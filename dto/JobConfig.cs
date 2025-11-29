namespace dy.net.dto
{
    /// <summary>
    /// 任务配置实体
    /// </summary>
    public class JobConfig
    {
        public JobConfig(Type jobType, string jobKey, string triggerKey, string description)
        {
            JobType = jobType ?? throw new ArgumentNullException(nameof(jobType));
            JobKey = jobKey ?? throw new ArgumentNullException(nameof(jobKey));
            TriggerKey = triggerKey ?? throw new ArgumentNullException(nameof(triggerKey));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        /// <summary>
        /// 任务类型
        /// </summary>
        public Type JobType { get; }

        /// <summary>
        /// 任务Key
        /// </summary>
        public string JobKey { get; }

        /// <summary>
        /// 触发器Key
        /// </summary>
        public string TriggerKey { get; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; }
    }
}
