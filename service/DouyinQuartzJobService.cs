using dy.net.job;
using dy.net.model.dto;
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
        private const string DefaultJobGroup = "dysync.net";
        private const int DefaultIntervalMinutes = 30;
        private const int DefaultCronStartDelaySeconds = 30;
        private const int DefaultSimpleStartDelaySeconds = 3;

        // 任务配置信息（修复了series任务的Key重复问题，确保每个任务Key唯一）
        public static Dictionary<string, JobConfig> JobConfigs { get; } = new()
        {
            {
                VideoTypeEnum.dy_collects.GetDesc(),
                new JobConfig(
                    typeof(DouyinCollectSyncJob),
                    "dy.job.key.collect",
                    "dy.trigger.key.collect",
                    "抖音收藏同步任务")
            },
            {
                VideoTypeEnum.dy_favorite.GetDesc(),
                new JobConfig(
                    typeof(DouyinFavoritSyncJob),
                    "dy.job.key.favorite",
                    "dy.trigger.key.favorite",
                    "抖音点赞同步任务")
            },
            {
                VideoTypeEnum.dy_follows.GetDesc(),
                new JobConfig(
                    typeof(DouyinFollowedSyncJob),
                    "dy.job.key.followed",
                    "dy.trigger.key.followed",
                    "抖音关注博主作品同步任务")
            },
            {
                "关注列表",
                new JobConfig(
                    typeof(DouyinFollowsAndCollnectsSyncJob),
                    "dy.job.key.follow_user",
                    "dy.trigger.key.follow_user",
                    "抖音关注列表同步任务")
            },
            {
                VideoTypeEnum.dy_custom_collect.GetDesc(),
                new JobConfig(
                    typeof(DouyinCollectCustomSyncJob),
                    "dy.job.key.custom_collect",
                    "dy.trigger.key.custom_collect",
                    "抖音自定义收藏夹列表同步任务")
            },
            {
               VideoTypeEnum.dy_mix.GetDesc(),
                new JobConfig(
                    typeof(DouyinMixSyncJob),
                    "dy.job.key.mix",
                    "dy.trigger.key.mix",
                    "抖音收藏夹合集同步任务")
            },
            {
                VideoTypeEnum.dy_series.GetDesc(),
                new JobConfig(
                    typeof(DouyinSeriesSyncJob),
                    "dy.job.key.series",
                    "dy.trigger.key.series",
                    "抖音收藏夹短剧同步任务")
            },
            {
                "follow_user_once",
                new JobConfig(
                    typeof(DouyinFollowsAndCollnectsSyncJob),
                    "dy.job.key.follow_user_once",
                    "dy.trigger.key.follow_user_once",
                    "抖音关注同步任务(单次执行)")
            }
        };

        public DouyinQuartzJobService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
        }

        /// <summary>
        /// 启动所有抖音相关定时任务（所有任务独立执行）
        /// </summary>
        /// <param name="expression">Cron表达式或间隔分钟数（所有任务使用相同的执行频率）</param>
        /// <returns>是否启动成功</returns>
        public async Task<bool> InitOrReStartAllJobs(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                Log.Debug("定时任务表达式为空，使用默认配置（{DefaultMinutes}分钟）", DefaultIntervalMinutes);
                expression = DefaultIntervalMinutes.ToString();
            }

            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();

                // 移除所有已存在的任务（避免重复调度）
                await RemoveAllExistingJobs(scheduler);


                // 执行任务启动逻辑
                foreach (var jobKey in JobConfigs.Keys)
                {
                    if (jobKey == "follow_user_once")
                        continue;
                    if (jobKey == "follow_user") expression = "60";
                    var startSuccess = await StartJobAsync(jobKey, expression);
                    if (startSuccess)
                    {
                        Log.Debug($"成功启动任务：{jobKey}，执行频率：{expression}");
                    }
                    else
                    {
                        Log.Error($"启动任务失败：{jobKey}");
                    }
                }
                Log.Information($"共启动 {JobConfigs.Count - 1} 个定时任务");


                //await StartJobAsync(VideoTypeEnum.dy_custom_collect.GetDesc(), expression);
                //await StartFollowJobOnceAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "【任务服务】初始化所有定时任务异常");
                return false;
            }
        }

        /// <summary>
        /// 启动关注同步任务（单次执行）
        /// </summary>
        public async Task<bool> StartFollowJobOnceAsync()
        {
            return await StartOneTimeJobAsync("follow_user_once");
        }

        /// <summary>
        /// 移除所有已存在的任务（避免重复调度）
        /// </summary>
        private async Task RemoveAllExistingJobs(IScheduler scheduler)
        {
            var jobKeys = JobConfigs.Values.Select(config => new JobKey(config.JobKey, DefaultJobGroup)).ToList();
            foreach (var jobKey in jobKeys)
            {
                if (await scheduler.CheckExists(jobKey))
                {
                    Log.Information("【任务服务】移除已存在的任务: {JobKey}", jobKey);
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
        public async Task<bool> StartJobAsync(string configKey, string expression)
        {
            if (!JobConfigs.TryGetValue(configKey, out var jobConfig))
            {
                Log.Error("【任务服务】找不到任务配置: {ConfigKey}", configKey);
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
                Log.Information("【任务服务】启动任务成功 - 任务描述: {JobDescription}, 执行频率: {Expression}",
                    jobConfig.Description,
                    expression);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "【任务服务】启动任务失败 - 任务描述: {JobDescription}", jobConfig.Description);
                return false;
            }
        }

        /// <summary>
        /// 启动单次执行任务
        /// </summary>
        private async Task<bool> StartOneTimeJobAsync(string configKey)
        {
            if (!JobConfigs.TryGetValue(configKey, out var jobConfig))
            {
                Log.Error("【任务服务】找不到任务配置: {ConfigKey}", configKey);
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
                Log.Information("【任务服务】启动单次任务成功 - 任务描述: {JobDescription}", jobConfig.Description);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "【任务服务】启动单次任务失败 - 任务描述: {JobDescription}", jobConfig.Description);
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
            Log.Warning("【任务服务】无效的任务表达式: {Expression}，使用默认间隔{DefaultMinutes}分钟",
                expression, DefaultIntervalMinutes);

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
                Log.Information("【任务服务】移除已存在的任务: {JobKey}", jobKey);
                await scheduler.DeleteJob(jobKey);
            }
        }
    }
}