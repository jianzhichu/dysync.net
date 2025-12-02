using dy.net.dto;
using dy.net.job;
using Quartz;
using Serilog;
using System;
using System.Threading.Tasks;

namespace dy.net.service
{
    /// <summary>
    /// 抖音相关定时任务服务
    /// </summary>
    public class DouyinQuartzJobService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private const string DefaultJobGroup = "group1";
        private const int DefaultIntervalMinutes = 30;
        private const int DefaultCronStartDelaySeconds = 30;
        private const int DefaultSimpleStartDelaySeconds = 3;

      

        public DouyinQuartzJobService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
        }

        /// <summary>
        /// 启动所有抖音相关定时任务
        /// </summary>
        /// <param name="expression">Cron表达式或间隔分钟数</param>
        /// <param name="delayBetweenJobs">任务之间的启动延迟(毫秒)</param>
        /// <returns>是否启动成功</returns>
        public async Task<bool> InitOrReStartAllJobs(string expression, int delayBetweenJobs = 5000)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                Log.Warning("定时任务表达式为空，使用默认配置");
                expression = DefaultIntervalMinutes.ToString();
            }

            // 按顺序启动任务，避免并发
            var jobTasks = new List<Task<bool>>
            {
                   //关注列表
                //StartJobAsync("follow_user", expression),
                //我收藏的作品
                StartJobAsync("collect", expression),
                //我喜欢的作品
                DelayAndStartJobAsync("favorite", expression, delayBetweenJobs),
                //关注的用户的作品
                DelayAndStartJobAsync("uper", expression, delayBetweenJobs * 2),
                //关注列表
                DelayAndStartJobAsync("follow_user", (Convert.ToInt32(expression)*2*24).ToString(), delayBetweenJobs * 3)
            };

            var results = await Task.WhenAll(jobTasks);
            return results.All(success => success);
        }

        /// <summary>
        /// 启动关注同步任务（单次执行）
        /// </summary>
        /// <returns>是否启动成功</returns>
        public async Task<bool> StartFollowJobOnceAsync()
        {
            return await StartOneTimeJobAsync("follow_user_once");
        }


        /// <summary>
        /// 延迟后启动任务
        /// </summary>
        private async Task<bool> DelayAndStartJobAsync(string jobKey, string expression, int delayMs)
        {
            if (delayMs > 0)
            {
                await Task.Delay(delayMs);
            }

            // 如果是数字表达式，自动递增避免并发
            var adjustedExpression = AdjustExpressionForConcurrency(jobKey, expression);
            return await StartJobAsync(jobKey, adjustedExpression);
        }

        /// <summary>
        /// 调整任务表达式以避免并发
        /// </summary>
        private string AdjustExpressionForConcurrency(string jobKey, string expression)
        {
            if (!int.TryParse(expression, out int interval))
                return expression;

            // 根据任务类型递增间隔，避免所有任务同时执行
            var jobIndex = _jobConfigs.Keys.ToList().IndexOf(jobKey);
            return (interval + jobIndex).ToString();
        }

        /// <summary>
        /// 启动指定定时任务
        /// </summary>
        private async Task<bool> StartJobAsync(string configKey, string expression)
        {
            if (!_jobConfigs.TryGetValue(configKey, out var jobConfig))
            {
                Log.Error("找不到任务配置: {ConfigKey}", configKey);
                return false;
            }

            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey(jobConfig.JobKey, DefaultJobGroup);
                var triggerKey = new TriggerKey(jobConfig.TriggerKey, DefaultJobGroup);

                // 删除已存在的任务
                await RemoveExistingJobAsync(scheduler, jobKey);

                // 创建任务详情
                var jobDetail = JobBuilder.Create(jobConfig.JobType)
                    .WithIdentity(jobKey)
                    .WithDescription(jobConfig.Description)
                    .Build();

                // 创建触发器
                var trigger = CreateTrigger(triggerKey, expression, jobConfig.Description);
                if (trigger == null)
                {
                    Log.Error("创建触发器失败: {JobDescription}", jobConfig.Description);
                    return false;
                }

                // 调度任务
                await scheduler.ScheduleJob(jobDetail, trigger);
                Log.Information("启动定时任务成功 - {JobDescription}, 表达式: {Expression}",
                    jobConfig.Description, expression);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "启动定时任务失败 - {JobDescription}", jobConfig.Description);
                return false;
            }
        }

        /// <summary>
        /// 启动单次执行任务
        /// </summary>
        private async Task<bool> StartOneTimeJobAsync(string configKey)
        {
            if (!_jobConfigs.TryGetValue(configKey, out var jobConfig))
            {
                Log.Error("找不到任务配置: {ConfigKey}", configKey);
                return false;
            }

            try
            {
                var scheduler = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey(jobConfig.JobKey, DefaultJobGroup);
                var triggerKey = new TriggerKey(jobConfig.TriggerKey, DefaultJobGroup);

                // 删除已存在的任务
                await RemoveExistingJobAsync(scheduler, jobKey);

                // 创建任务详情
                var jobDetail = JobBuilder.Create(jobConfig.JobType)
                    .WithIdentity(jobKey)
                    .WithDescription(jobConfig.Description)
                    .Build();

                // 创建立即执行的触发器（只执行一次）
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .WithDescription($"{jobConfig.Description} - 单次执行")
                    .StartNow()
                    .Build();

                // 调度任务
                await scheduler.ScheduleJob(jobDetail, trigger);
                Log.Information("启动单次任务成功 - {JobDescription}", jobConfig.Description);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "启动单次任务失败 - {JobDescription}", jobConfig.Description);
                return false;
            }
        }

        /// <summary>
        /// 创建触发器（支持Cron表达式和简单间隔）
        /// </summary>
        private ITrigger? CreateTrigger(TriggerKey triggerKey, string expression, string jobDescription)
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
                    .WithDescription($"{jobDescription} - 间隔{intervalMinutes}分钟")
                    .StartAt(DateTime.Now.AddSeconds(DefaultSimpleStartDelaySeconds))
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(intervalMinutes)
                        .RepeatForever())
                    .Build();
            }

            // 无效表达式，使用默认配置
            Log.Warning("无效的任务表达式: {Expression}，使用默认间隔{DefaultMinutes}分钟",
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
        private async Task RemoveExistingJobAsync(IScheduler scheduler, JobKey jobKey)
        {
            if (await scheduler.CheckExists(jobKey))
            {
                Log.Information("移除已存在的任务: {JobKey}", jobKey);
                await scheduler.DeleteJob(jobKey);
            }
        }
        /// <summary>
        /// 任务配置信息
        /// </summary>
        private readonly Dictionary<string, JobConfig> _jobConfigs = new()
        {
            {
                "collect",
                new JobConfig(
                    typeof(DouyinCollectSyncJob),
                    "dy.job.key.collect",
                    "dy.trigger.key.collect",
                    "抖音收藏同步任务")
            },
            {
                "favorite",
                new JobConfig(
                    typeof(DouyinFavoritSyncJob),
                    "dy.job.key.favorite",
                    "dy.trigger.key.favorite",
                    "抖音点赞同步任务")
            },
            {
                "uper",
                new JobConfig(
                    typeof(DouyinFollowedViedoSyncJob),
                    "dy.job.key.uper",
                    "dy.trigger.key.uper",
                    "抖音UP主作品同步任务")
            },
            {
                "follow_user",
                new JobConfig(
                    typeof(DouyinFollowedUsersSyncJob),
                    "dy.job.key.follow_user",
                    "dy.trigger.key.follow_user",
                    "抖音关注同步任务")
            },
            {
                "follow_user_once",
                new JobConfig(
                    typeof(DouyinFollowedUsersSyncJob),
                    "dy.job.key.follow_user_once",
                    "dy.trigger.key.follow_user_once",
                    "抖音关注同步任务(单次执行)")
            }
            ,
            //{
            //    "redown_once",
            //    new JobConfig(
            //        typeof(DouyinReDownSyncJob),
            //        "dy.job.key.redown_once",
            //        "dy.trigger.key.redown_once",
            //        "抖音重新下载任务(单次执行)")
            //}
        };

    }
}