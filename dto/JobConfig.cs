namespace dy.net.dto
{
    /// <summary>
    /// 任务配置实体
    /// </summary>
    //public class JobConfig
    //{
    //    public JobConfig(Type jobType, string jobKey, string triggerKey, string description)
    //    {
    //        JobType = jobType ?? throw new ArgumentNullException(nameof(jobType));
    //        JobKey = jobKey ?? throw new ArgumentNullException(nameof(jobKey));
    //        TriggerKey = triggerKey ?? throw new ArgumentNullException(nameof(triggerKey));
    //        Description = description ?? throw new ArgumentNullException(nameof(description));
    //    }

    //    /// <summary>
    //    /// 任务类型
    //    /// </summary>
    //    public Type JobType { get; }

    //    /// <summary>
    //    /// 任务Key
    //    /// </summary>
    //    public string JobKey { get; }

    //    /// <summary>
    //    /// 触发器Key
    //    /// </summary>
    //    public string TriggerKey { get; }

    //    /// <summary>
    //    /// 任务描述
    //    /// </summary>
    //    public string Description { get; }
    //}


    public class JobConfig
    {
        public Type JobType { get; }
        public string JobKey { get; }
        public string TriggerKey { get; }
        public string Description { get; }

        public JobConfig(Type jobType, string jobKey, string triggerKey, string description)
        {
            JobType = jobType ?? throw new ArgumentNullException(nameof(jobType), "任务类型不能为空");
            JobKey = jobKey ?? throw new ArgumentNullException(nameof(jobKey), "任务Key不能为空");
            TriggerKey = triggerKey ?? throw new ArgumentNullException(nameof(triggerKey), "触发器Key不能为空");
            Description = description ?? throw new ArgumentNullException(nameof(description), "任务描述不能为空");
        }
    }
}
