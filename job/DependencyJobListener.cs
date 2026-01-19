using dy.net.model.dto;
using dy.net.service;
using Quartz;
using Serilog;
using static Quartz.Logging.OperationName;

namespace dy.net.job
{  /// <summary>
   /// 抖音任务依赖监听器（独立公共类）
   /// 作用：监听任务执行完成事件，触发下一个依赖任务，实现顺序执行
   /// </summary>
    public class DouyinJobDependencyListener : IJobListener
    {
        // 监听器名称（唯一标识，不可重复）
        public string Name => "DouyinJobDependencyListener";

        /// <summary>
        /// 任务配置字典（从外部注入）
        /// </summary>
        private readonly Dictionary<string, JobConfig> _jobConfigs;

        /// <summary>
        /// 任务依赖关系（从外部注入，定义执行顺序）
        /// </summary>
        private readonly Dictionary<string, string> _jobDependency;

        /// <summary>
        /// 任务服务（用于触发下一个任务，从外部注入）
        /// </summary>
        private readonly DouyinQuartzJobService _jobService;

        /// <summary>
        /// 构造函数（依赖注入）
        /// </summary>
        /// <param name="jobConfigs">任务配置</param>
        /// <param name="jobDependency">任务依赖关系</param>
        /// <param name="jobService">任务服务</param>
        public DouyinJobDependencyListener(
            Dictionary<string, JobConfig> jobConfigs,
            Dictionary<string, string> jobDependency,
            DouyinQuartzJobService jobService)
        {
            _jobConfigs = jobConfigs ?? throw new ArgumentNullException(nameof(jobConfigs), "任务配置不能为空");
            _jobDependency = jobDependency ?? throw new ArgumentNullException(nameof(jobDependency), "任务依赖关系不能为空");
            _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService), "任务服务不能为空");
        }

        /// <summary>
        /// 任务执行前触发（无需处理）
        /// </summary>
        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 任务被否决执行时触发（无需处理）
        /// </summary>
        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 任务执行完成后触发（核心逻辑：触发下一个依赖任务）
        /// </summary>
        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
        {
            var currentJobKey = context.JobDetail.Key;
            Log.Information("【任务监听】任务执行完成 - 任务名称: {JobName}, 执行状态: {Status}",
                currentJobKey.Name, jobException == null ? "成功" : "失败");

            // 1. 若当前任务执行失败，终止后续依赖任务（避免无效执行）
            if (jobException != null)
            {
                Log.Error(jobException, "【任务监听】任务 {JobName} 执行失败，终止后续任务链条", currentJobKey.Name);
                return;
            }

            // 2. 根据当前任务的 JobKey，找到对应的配置 Key（如：dy.job.key.collect → collect）
            var currentConfigKey = _jobConfigs.FirstOrDefault(kv => kv.Value.JobKey == currentJobKey.Name).Key;
            if (string.IsNullOrEmpty(currentConfigKey))
            {
                Log.Warning("【任务监听】未找到任务 {JobName} 的配置信息，任务链条终止", currentJobKey.Name);
                return;
            }

            // 3. 查找下一个依赖任务的配置 Key
            if (!_jobDependency.TryGetValue(currentConfigKey, out var nextConfigKey) || string.IsNullOrEmpty(nextConfigKey))
            {
                Log.Information("【任务监听】任务 {JobName} 是最后一个任务，本次任务链条执行完毕", currentJobKey.Name);
                return;
            }

            // 4. 触发下一个任务（标记为「依赖触发」，立即执行）
            Log.Information("【任务监听】准备触发下一个任务: {NextJobName}（依赖触发）", nextConfigKey);
            var triggerSuccess = await _jobService.StartJobAsync(nextConfigKey, "", isDependencyTrigger: true);

            if (triggerSuccess)
            {
                Log.Information("【任务监听】下一个任务 {NextJobName} 触发成功", nextConfigKey);
            }
            else
            {
                Log.Error("【任务监听】下一个任务 {NextJobName} 触发失败，任务链条中断", nextConfigKey);
            }
        }
    }
}
