using dy.net.job;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.utils;
using Quartz;
using Serilog;

namespace dy.net.service
{
    /// <summary>
    /// 抖音相关定时任务服务
    /// </summary>
    public class DouyinQuartzJobService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly DouyinCookieService douyinCookieService;
        private const string DefaultJobGroup = "dysync.net";
        private const int DefaultIntervalMinutes = 30;
        private const int DefaultCronStartDelaySeconds = 30;
        private const int DefaultSimpleStartDelaySeconds = 3;

        // 任务配置信息（修复了series任务的Key重复问题，确保每个任务Key唯一）
        public static Dictionary<VideoTypeEnum, JobConfig> JobConfigs { get; } = new()
        {
            {
                VideoTypeEnum.dy_collects,
                new JobConfig(
                    typeof(DouyinCollectSyncJob),
                    "dy.job.key.collect",
                    "dy.trigger.key.collect",
                    "抖音收藏同步任务")
            },
            {
                VideoTypeEnum.dy_favorite,
                new JobConfig(
                    typeof(DouyinFavoritSyncJob),
                    "dy.job.key.favorite",
                    "dy.trigger.key.favorite",
                    "抖音点赞同步任务")
            },
            {
                VideoTypeEnum.dy_follows,
                new JobConfig(
                    typeof(DouyinFollowedSyncJob),
                    "dy.job.key.followed",
                    "dy.trigger.key.followed",
                    "抖音关注博主作品同步任务")
            },
            {
                VideoTypeEnum.dy_followuser,
                new JobConfig(
                    typeof(DouyinFollowsAndCollnectsSyncJob),
                    "dy.job.key.follow_user",
                    "dy.trigger.key.follow_user",
                    "抖音关注列表同步任务")
            },
            {
                VideoTypeEnum.dy_custom_collect,
                new JobConfig(
                    typeof(DouyinCollectCustomSyncJob),
                    "dy.job.key.custom_collect",
                    "dy.trigger.key.custom_collect",
                    "抖音自定义收藏夹列表同步任务")
            },
            {
               VideoTypeEnum.dy_mix,
                new JobConfig(
                    typeof(DouyinMixSyncJob),
                    "dy.job.key.mix",
                    "dy.trigger.key.mix",
                    "抖音收藏夹合集同步任务")
            },
            {
                VideoTypeEnum.dy_series,
                new JobConfig(
                    typeof(DouyinSeriesSyncJob),
                    "dy.job.key.series",
                    "dy.trigger.key.series",
                    "抖音收藏夹短剧同步任务")
            },
            {
              VideoTypeEnum.dy_followuser_once,
                new JobConfig(
                    typeof(DouyinFollowsAndCollnectsSyncJob),
                    "dy.job.key.sync_follow_user_once",
                    "dy.trigger.key.sync_follow_user_once",
                    "抖音关注同步任务(单次执行)")
            }
        };

        public DouyinQuartzJobService(ISchedulerFactory schedulerFactory,DouyinCookieService douyinCookieService)
        {
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            this.douyinCookieService = douyinCookieService;
        }

       
        /// <summary>
        /// 初始化或重启所有抖音定时任务
        /// </summary>
        /// <param name="cronExpression">定时任务表达式（分钟数）</param>
        /// <returns>是否成功初始化</returns>
        public async Task<bool> InitOrReStartAllJobs(string cronExpression)
        {
            try
            {
                // 1. 获取并验证Cookie
                var validCookies = await douyinCookieService.GetOpendCookiesAsync();
                if (validCookies == null || !validCookies.Any())
                {
                    Serilog.Log.Debug("没有有效的抖音Cookie，无法启动定时任务");
                    return false;
                }

                // 2. 处理定时任务表达式
                var taskIntervalExpression = ResolveTaskExpression(cronExpression);

                // 3. 获取调度器并清理现有任务
                var scheduler = await _schedulerFactory.GetScheduler();
                if (scheduler == null)
                {
                    Log.Error("获取任务调度器失败，无法初始化定时任务");
                    return false;
                }

                await RemoveAllExistingJobs(scheduler);

                // 4. 检查各类型任务的启用条件
                var taskEnableConditions = GetTaskEnableConditions(validCookies);

                // 5. 启动符合条件的定时任务
                int successfullyStartedJobs = 0;
                foreach (var jobKey in JobConfigs.Keys)
                {
                    // 跳过一次性关注用户任务
                    if (jobKey == VideoTypeEnum.dy_followuser_once)
                        continue;

                    // 处理关注用户任务（固定60分钟执行频率）
                    if (jobKey == VideoTypeEnum.dy_followuser)
                    {
                        bool startSuccess = await StartSingleJobAsync(jobKey, "60");
                        if (startSuccess) successfullyStartedJobs++;
                        continue;
                    }

                    // 根据不同任务类型和启用条件启动任务
                    bool isTaskEnabled = jobKey switch
                    {
                        VideoTypeEnum.dy_favorite => taskEnableConditions.IsFavoriteEnabled,
                        VideoTypeEnum.dy_collects => taskEnableConditions.IsCollectEnabled,
                        VideoTypeEnum.dy_follows => taskEnableConditions.IsFollowedEnabled,
                        VideoTypeEnum.dy_custom_collect => taskEnableConditions.IsCustomCollectEnabled,
                        VideoTypeEnum.dy_mix => taskEnableConditions.IsMixEnabled,
                        VideoTypeEnum.dy_series => taskEnableConditions.IsSeriesEnabled,
                        _ => false
                    };

                    if (isTaskEnabled)
                    {
                        bool startSuccess = await StartSingleJobAsync(jobKey, taskIntervalExpression);
                        if (startSuccess) successfullyStartedJobs++;
                    }
                }

                // 6. 输出任务启动统计日志
                Log.Information($"定时任务初始化完成，共尝试启动 {JobConfigs.Count - 1} 个任务，成功启动 {successfullyStartedJobs} 个");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "【quartz】初始化所有抖音定时任务时发生异常");
                return false;
            }
        }


        /// <summary>
        /// 解析任务执行表达式，为空时使用默认值
        /// </summary>
        /// <param name="inputExpression">输入的表达式</param>
        /// <returns>处理后的表达式</returns>
        private static string ResolveTaskExpression(string inputExpression)
        {
            if (string.IsNullOrWhiteSpace(inputExpression))
            {
                Log.Debug("定时任务表达式为空，使用默认配置（{DefaultMinutes}分钟）", DefaultIntervalMinutes);
                return DefaultIntervalMinutes.ToString();
            }
            return inputExpression;
        }

        /// <summary>
        /// 获取各类型任务的启用条件
        /// </summary>
        /// <param name="cookies">有效的抖音Cookie列表</param>
        /// <returns>任务启用条件集合</returns>
        private static TaskEnableConditions GetTaskEnableConditions(IEnumerable<DouyinCookie> cookies)
        {
            return new TaskEnableConditions
            {
                IsCollectEnabled = cookies.Any(x => x.DownCollect && !x.UseCollectFolder && !string.IsNullOrWhiteSpace(x.SavePath)),
                IsFavoriteEnabled = cookies.Any(x => x.DownFavorite && !string.IsNullOrWhiteSpace(x.FavSavePath)),
                IsFollowedEnabled = cookies.Any(x => x.DownFollowd && !string.IsNullOrWhiteSpace(x.UpSavePath)),
                IsMixEnabled = cookies.Any(x => x.DownMix && !string.IsNullOrWhiteSpace(x.MixPath)),
                IsSeriesEnabled = cookies.Any(x => x.DownSeries && !string.IsNullOrWhiteSpace(x.SeriesPath)),
                IsCustomCollectEnabled = cookies.Any(x => x.UseCollectFolder && !string.IsNullOrWhiteSpace(x.SavePath))
            };
        }

        /// <summary>
        /// 启动单个定时任务（封装重复的启动逻辑）
        /// </summary>
        /// <param name="jobKey">任务类型</param>
        /// <param name="expression">执行频率表达式</param>
        /// <returns>是否启动成功</returns>
        private async Task<bool> StartSingleJobAsync(VideoTypeEnum jobKey, string expression)
        {
            try
            {
                bool startSuccess = await StartJobAsync(jobKey, expression);
                if (startSuccess)
                {
                    Log.Debug($"【quartz】成功启动任务：{jobKey}，执行频率：{expression}分钟");
                }
                else
                {
                    Log.Error($"【quartz】启动任务失败：{jobKey}");
                }
                return startSuccess;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"【quartz】启动任务 {jobKey} 时发生异常");
                return false;
            }
        }
        /// <summary>
        /// 启动关注同步任务（单次执行）
        /// </summary>
        public async Task<bool> StartFollowJobOnceAsync()
        {
            return await StartOneTimeJobAsync(VideoTypeEnum.dy_followuser_once);
        }

        /// <summary>
        /// 移除所有已存在的任务（避免重复调度）
        /// </summary>
        private static async Task RemoveAllExistingJobs(IScheduler scheduler)
        {
            var jobKeys = JobConfigs.Values.Select(config => new JobKey(config.JobKey, DefaultJobGroup)).ToList();
            foreach (var jobKey in jobKeys)
            {
                if (await scheduler.CheckExists(jobKey))
                {
                    //Log.Debug("【quartz】移除已存在的任务: {JobKey}", jobKey);
                    await scheduler.DeleteJob(jobKey);
                }
            }
        }

        /// <summary>
        /// 启动指定定时任务（独立执行，无依赖触发）
        /// </summary>
        /// <param name="configKey">任务配置Key（如：collect、favorite）</param>
        /// <param name="expression">定时表达式（Cron或间隔分钟数）</param>
        /// <returns>是否启动成功</returns>
        public async Task<bool> StartJobAsync(VideoTypeEnum configKey, string expression)
        {
            if (!JobConfigs.TryGetValue(configKey, out var jobConfig))
            {
                Log.Error("【quartz】找不到任务配置: {ConfigKey}", configKey);
                return false;
            }

            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey(jobConfig.JobKey, DefaultJobGroup);
                var triggerKey = new TriggerKey(jobConfig.TriggerKey, DefaultJobGroup);

                // 移除已存在的任务（防止重复执行）
                await RemoveExistingJobAsync(scheduler, jobKey);

                // 创建任务详情（保留禁止并发执行，避免同一任务重复运行）
                var jobDetail = JobBuilder.Create(jobConfig.JobType)
                    .WithIdentity(jobKey)
                    .WithDescription(jobConfig.Description)
                    .DisallowConcurrentExecution() // 禁止同一任务并发执行
                    .Build();

                // 创建定时触发器（仅使用定时触发，移除依赖触发逻辑）
                ITrigger trigger = CreateScheduledTrigger(triggerKey, expression, jobConfig.Description);

                // 调度任务
                await scheduler.ScheduleJob(jobDetail, trigger);
                Log.Information("【quartz】启动任务成功 - 任务描述: {JobDescription}, 执行频率: {Expression}",
                    jobConfig.Description,
                    expression);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "【quartz】启动任务失败 - 任务描述: {JobDescription}", jobConfig.Description);
                return false;
            }
        }

        /// <summary>
        /// 启动单次执行任务
        /// </summary>
        private async Task<bool> StartOneTimeJobAsync(VideoTypeEnum configKey)
        {
            if (!JobConfigs.TryGetValue(configKey, out var jobConfig))
            {
                Log.Error("【quartz】找不到任务配置: {ConfigKey}", configKey);
                return false;
            }

            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey(jobConfig.JobKey, DefaultJobGroup);
                var triggerKey = new TriggerKey(jobConfig.TriggerKey, DefaultJobGroup);

                await RemoveExistingJobAsync(scheduler, jobKey);

                var jobDetail = JobBuilder.Create(jobConfig.JobType)
                    .WithIdentity(jobKey)
                    .WithDescription(jobConfig.Description)
                    .DisallowConcurrentExecution()
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .WithDescription($"{jobConfig.Description} - 单次执行")
                    .StartNow()
                    .Build();

                await scheduler.ScheduleJob(jobDetail, trigger);
                Log.Debug("【quartz】启动单次任务成功 - 任务描述: {JobDescription}", jobConfig.Description);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "【quartz】启动单次任务失败 - 任务描述: {JobDescription}", jobConfig.Description);
                return false;
            }
        }

        /// <summary>
        /// 创建定时触发器（支持Cron表达式或分钟间隔）
        /// </summary>
        private static ITrigger CreateScheduledTrigger(TriggerKey triggerKey, string expression, string jobDescription)
        {
            // Cron表达式格式
            if (CronExpression.IsValidExpression(expression))
            {
                return TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .WithDescription($"{jobDescription} - Cron调度")
                    .WithCronSchedule(expression)
                    .StartAt(DateTime.Now.AddSeconds(DefaultCronStartDelaySeconds))
                    .Build();
            }

            // 数字间隔格式（分钟）
            if (int.TryParse(expression, out int intervalMinutes))
            {
                intervalMinutes = Math.Max(1, intervalMinutes); // 最小间隔1分钟
                return TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .WithDescription($"{jobDescription} - 间隔{intervalMinutes}分钟调度")
                    .StartAt(DateTime.Now.AddSeconds(DefaultSimpleStartDelaySeconds))
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(intervalMinutes)
                        .RepeatForever())
                    .Build();
            }

            // 无效表达式，使用默认配置
            //Log.Debug("【任务服务】无效的任务表达式: {Expression}，使用默认间隔{DefaultMinutes}分钟",
            //    expression, DefaultIntervalMinutes);

            return TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithDescription($"{jobDescription} - 默认间隔调度")
                .StartAt(DateTime.Now.AddSeconds(DefaultSimpleStartDelaySeconds))
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(DefaultIntervalMinutes)
                    .RepeatForever())
                .Build();
        }

        /// <summary>
        /// 移除已存在的任务
        /// </summary>
        private static async Task RemoveExistingJobAsync(IScheduler scheduler, JobKey jobKey)
        {
            if (await scheduler.CheckExists(jobKey))
            {
                //Log.Debug("【quartz】移除已存在的任务: {JobKey}", jobKey);
                await scheduler.DeleteJob(jobKey);
            }
        }




        /// <summary>
        /// 任务启用条件模型
        /// </summary>
        private class TaskEnableConditions
        {
            public bool IsCollectEnabled { get; set; }
            public bool IsFavoriteEnabled { get; set; }
            public bool IsFollowedEnabled { get; set; }
            public bool IsMixEnabled { get; set; }
            public bool IsSeriesEnabled { get; set; }
            public bool IsCustomCollectEnabled { get; set; }
        }
    }
}