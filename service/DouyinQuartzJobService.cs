using dy.net.dto;
using dy.net.job;
using Quartz;
using Quartz.Impl.Matchers;
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
        private const string DefaultJobGroup = "dysync.net";
        private const int DefaultIntervalMinutes = 30;
        private const int DefaultCronStartDelaySeconds = 30;
        private const int DefaultSimpleStartDelaySeconds = 3;

        // 任务顺序依赖配置（核心：定义执行顺序，供 Listener 使用）
        public Dictionary<string, string> JobDependency { get; } = new()
        {
            {"collect", "favorite"},       // collect 执行完 → 触发 favorite
            {"favorite", "followed"},          // favorite 执行完 → 触发 followed
            {"followed", null},       // followed 执行完 → 一轮任务结束
        };

        // 任务配置信息（保持原有配置不变，改为 public 供 Listener 访问）
        public Dictionary<string, JobConfig> JobConfigs { get; } = new()
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
                "followed",
                new JobConfig(
                    typeof(DouyinFollowedViedoSyncJob),
                    "dy.job.key.followed",
                    "dy.trigger.key.followed",
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
        };

        public DouyinQuartzJobService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
        }

        /// <summary>
        /// 启动所有抖音相关定时任务（顺序执行模式）
        /// </summary>
        /// <param name="expression">Cron表达式或间隔分钟数（控制整个链条的执行频率）</param>
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

                // 1. 注册独立的 JobListener（核心：注入配置和服务）
                await RegisterJobListener(scheduler);

                // 2. 移除所有已存在的任务（避免重复调度）
                await RemoveAllExistingJobs(scheduler);

                // 3. 只启动第一个任务（collect），后续任务由 Listener 自动触发
                var firstJobConfigKey = "collect";
                var startSuccess =  await StartJobAsync(firstJobConfigKey, expression);

                if (startSuccess)
                {
                    Log.Debug($"同步任务执行顺序：collect → favorite → followed-->默认每{expression}分钟执行一次...");
                }
                else
                {
                    Log.Error($"任务执行失败（任务 {firstJobConfigKey} 启动失败）");
                }
                //启动follow_user--这个与其他几个任务没有依赖关系，所以单独启动
                await StartJobAsync("follow_user", expression);
                return startSuccess;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "【任务服务】初始化任务链条异常");
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
        /// 注册独立的 JobListener（核心步骤）
        /// </summary>
        private async Task RegisterJobListener(IScheduler scheduler)
        {
            // 创建独立的 Listener 实例，注入依赖（任务配置、依赖关系、当前服务）
            var dependencyListener = new DouyinJobDependencyListener(
                JobConfigs,  // 任务配置
                JobDependency,  // 依赖顺序
                this  // 任务服务（用于触发下一个任务）
            );

            // 注册 Listener：仅监听 DefaultJobGroup 分组的任务（精准匹配，避免影响其他任务）
            scheduler.ListenerManager.AddJobListener(
                dependencyListener,
                GroupMatcher<JobKey>.GroupEquals(DefaultJobGroup)
            );

            Log.Information("【任务服务】JobListener 注册成功：{ListenerName}", dependencyListener.Name);
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
        /// 启动指定定时任务（public 修饰，供 Listener 调用）
        /// </summary>
        /// <param name="configKey">任务配置Key（如：collect、favorite）</param>
        /// <param name="expression">定时表达式（依赖触发时传空）</param>
        /// <param name="isDependencyTrigger">是否为依赖触发（true=立即执行，false=定时执行）</param>
        /// <returns>是否启动成功</returns>
        public async Task<bool> StartJobAsync(string configKey, string expression, bool isDependencyTrigger = false)
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
                // 触发器Key：区分「定时触发」和「依赖触发」，避免冲突
                var triggerKey = new TriggerKey(
                    $"{jobConfig.TriggerKey}_{(isDependencyTrigger ? "dependency" : "main")}",
                    DefaultJobGroup
                );

                // 移除已存在的任务（防止重复执行）
                await RemoveExistingJobAsync(scheduler, jobKey);

                // 创建任务详情（添加禁止并发执行特性，避免顺序混乱）
                var jobDetail = JobBuilder.Create(jobConfig.JobType)
                    .WithIdentity(jobKey)
                    .WithDescription(jobConfig.Description)
                    .DisallowConcurrentExecution() // 关键：禁止同一任务并发执行
                    .Build();

                // 创建立触发器
                ITrigger trigger = isDependencyTrigger
                    ? CreateDependencyTrigger(triggerKey, jobConfig.Description) // 依赖触发：立即执行
                    : CreateScheduledTrigger(triggerKey, expression, jobConfig.Description); // 定时触发：按表达式执行

                // 调度任务
                await scheduler.ScheduleJob(jobDetail, trigger);
                Log.Information("【任务服务】启动任务成功 - 任务描述: {JobDescription}, 触发类型: {TriggerType}, 表达式: {Expression}",
                    jobConfig.Description,
                    isDependencyTrigger ? "依赖触发（立即执行）" : "定时触发",
                    isDependencyTrigger ? "无" : expression);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "【任务服务】启动任务失败 - 任务描述: {JobDescription}", jobConfig.Description);
                return false;
            }
        }

        /// <summary>
        /// 启动单次执行任务（保持原有逻辑不变）
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
        /// 创建「定时触发器」（按表达式执行，仅第一个任务使用）
        /// </summary>
        private ITrigger CreateScheduledTrigger(TriggerKey triggerKey, string expression, string jobDescription)
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
        /// 创建「依赖触发器」（立即执行，仅执行一次）
        /// </summary>
        private ITrigger CreateDependencyTrigger(TriggerKey triggerKey, string jobDescription)
        {
            return TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithDescription($"{jobDescription} - 依赖触发（立即执行）")
                .StartNow() // 立即触发
                .Build();
        }

        /// <summary>
        /// 移除已存在的任务（保持原有逻辑不变）
        /// </summary>
        private async Task RemoveExistingJobAsync(IScheduler scheduler, JobKey jobKey)
        {
            if (await scheduler.CheckExists(jobKey))
            {
                Log.Information("【任务服务】移除已存在的任务: {JobKey}", jobKey);
                await scheduler.DeleteJob(jobKey);
            }
        }

}
}