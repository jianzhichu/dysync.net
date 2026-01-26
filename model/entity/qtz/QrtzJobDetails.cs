using SqlSugar;

namespace dy.net.model.entity
{

    #region QRTZ_JOB_DETAILS（任务详情表）
    [SugarTable("QRTZ_JOB_DETAILS")]
    public class QrtzJobDetails
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "JOB_NAME", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string JobName { get; set; }

        [SugarColumn(ColumnName = "JOB_GROUP", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string JobGroup { get; set; }

        [SugarColumn(ColumnName = "DESCRIPTION", Length = 250, IsNullable = true)]
        public string? Description { get; set; }

        [SugarColumn(ColumnName = "JOB_CLASS_NAME", Length = 250, IsNullable = true)]
        public string JobClassName { get; set; }

        [SugarColumn(ColumnName = "IS_DURABLE", IsNullable = true)]
        public bool IsDurable { get; set; }

        [SugarColumn(ColumnName = "IS_NONCONCURRENT", IsNullable = true)]
        public bool IsNonConcurrent { get; set; }

        [SugarColumn(ColumnName = "IS_UPDATE_DATA", IsNullable = true)]
        public bool IsUpdateData { get; set; }

        [SugarColumn(ColumnName = "REQUESTS_RECOVERY", IsNullable = true)]
        public bool RequestsRecovery { get; set; }

        [SugarColumn(ColumnName = "JOB_DATA", IsNullable = true, ColumnDataType = "BLOB")]
        public byte[]? JobData { get; set; }

        // 导航属性：关联触发器（一对多）
        [Navigate(NavigateType.OneToMany,
            nameof(QrtzTriggers.SchedName),
            nameof(QrtzTriggers.JobName),
            nameof(QrtzTriggers.JobGroup))]
        public List<QrtzTriggers> Triggers { get; set; } = new List<QrtzTriggers>();
    }
    #endregion

    #region QRTZ_TRIGGERS（触发器表）
    [SugarTable("QRTZ_TRIGGERS")]
    public class QrtzTriggers
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_NAME", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_GROUP", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerGroup { get; set; }

        [SugarColumn(ColumnName = "JOB_NAME", Length = 150, IsNullable = true)]
        public string JobName { get; set; }

        [SugarColumn(ColumnName = "JOB_GROUP", Length = 150, IsNullable = true)]
        public string JobGroup { get; set; }

        [SugarColumn(ColumnName = "DESCRIPTION", Length = 250, IsNullable = true)]
        public string? Description { get; set; }

        [SugarColumn(ColumnName = "NEXT_FIRE_TIME", IsNullable = true)]
        public long? NextFireTime { get; set; }

        [SugarColumn(ColumnName = "PREV_FIRE_TIME", IsNullable = true)]
        public long? PrevFireTime { get; set; }

        [SugarColumn(ColumnName = "PRIORITY", IsNullable = true)]
        public int? Priority { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_STATE", Length = 16, IsNullable = true)]
        public string TriggerState { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_TYPE", Length = 8, IsNullable = true)]
        public string TriggerType { get; set; }

        [SugarColumn(ColumnName = "START_TIME", IsNullable = true)]
        public long StartTime { get; set; }

        [SugarColumn(ColumnName = "END_TIME", IsNullable = true)]
        public long? EndTime { get; set; }

        [SugarColumn(ColumnName = "CALENDAR_NAME", Length = 200, IsNullable = true)]
        public string? CalendarName { get; set; }

        [SugarColumn(ColumnName = "MISFIRE_INSTR", IsNullable = true)]
        public int? MisfireInstr { get; set; }

        [SugarColumn(ColumnName = "JOB_DATA", IsNullable = true, ColumnDataType = "BLOB")]
        public byte[]? JobData { get; set; }

        // 导航属性：关联任务详情（多对一）
        [Navigate(NavigateType.ManyToOne,
            nameof(SchedName),
            nameof(JobName),
            nameof(JobGroup))]
        public QrtzJobDetails? JobDetails { get; set; }

        // 导航属性：关联各类触发器（一对一）
        [Navigate(NavigateType.OneToOne,
            nameof(SchedName),
            nameof(TriggerName),
            nameof(TriggerGroup))]
        public QrtzSimpleTriggers? SimpleTrigger { get; set; }

        [Navigate(NavigateType.OneToOne,
            nameof(SchedName),
            nameof(TriggerName),
            nameof(TriggerGroup))]
        public QrtzSimpropTriggers? SimpropTrigger { get; set; }

        [Navigate(NavigateType.OneToOne,
            nameof(SchedName),
            nameof(TriggerName),
            nameof(TriggerGroup))]
        public QrtzCronTriggers? CronTrigger { get; set; }

        [Navigate(NavigateType.OneToOne,
            nameof(SchedName),
            nameof(TriggerName),
            nameof(TriggerGroup))]
        public QrtzBlobTriggers? BlobTrigger { get; set; }
    }
    #endregion

    #region QRTZ_SIMPLE_TRIGGERS（简单触发器表）
    [SugarTable("QRTZ_SIMPLE_TRIGGERS")]
    public class QrtzSimpleTriggers
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_NAME", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_GROUP", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerGroup { get; set; }

        [SugarColumn(ColumnName = "REPEAT_COUNT", IsNullable = true)]
        public long RepeatCount { get; set; }

        [SugarColumn(ColumnName = "REPEAT_INTERVAL", IsNullable = true)]
        public long RepeatInterval { get; set; }

        [SugarColumn(ColumnName = "TIMES_TRIGGERED", IsNullable = true)]
        public long TimesTriggered { get; set; }

        // 导航属性：关联触发器（多对一）
        [Navigate(NavigateType.ManyToOne,
            nameof(SchedName),
            nameof(TriggerName),
            nameof(TriggerGroup))]
        public QrtzTriggers? Trigger { get; set; }
    }
    #endregion

    #region QRTZ_SIMPROP_TRIGGERS（简单属性触发器表）
    [SugarTable("QRTZ_SIMPROP_TRIGGERS")]
    public class QrtzSimpropTriggers
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_NAME", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_GROUP", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerGroup { get; set; }

        [SugarColumn(ColumnName = "STR_PROP_1", Length = 512, IsNullable = true)]
        public string? StrProp1 { get; set; }

        [SugarColumn(ColumnName = "STR_PROP_2", Length = 512, IsNullable = true)]
        public string? StrProp2 { get; set; }

        [SugarColumn(ColumnName = "STR_PROP_3", Length = 512, IsNullable = true)]
        public string? StrProp3 { get; set; }

        [SugarColumn(ColumnName = "INT_PROP_1", IsNullable = true)]
        public int? IntProp1 { get; set; }

        [SugarColumn(ColumnName = "INT_PROP_2", IsNullable = true)]
        public int? IntProp2 { get; set; }

        [SugarColumn(ColumnName = "LONG_PROP_1", IsNullable = true)]
        public long? LongProp1 { get; set; }

        [SugarColumn(ColumnName = "LONG_PROP_2", IsNullable = true)]
        public long? LongProp2 { get; set; }

        [SugarColumn(ColumnName = "DEC_PROP_1", IsNullable = true)]
        public decimal? DecProp1 { get; set; }

        [SugarColumn(ColumnName = "DEC_PROP_2", IsNullable = true)]
        public decimal? DecProp2 { get; set; }

        [SugarColumn(ColumnName = "BOOL_PROP_1", IsNullable = true)]
        public bool? BoolProp1 { get; set; }

        [SugarColumn(ColumnName = "BOOL_PROP_2", IsNullable = true)]
        public bool? BoolProp2 { get; set; }

        [SugarColumn(ColumnName = "TIME_ZONE_ID", Length = 80, IsNullable = true)]
        public string? TimeZoneId { get; set; }

        // 导航属性：关联触发器（多对一）
        [Navigate(NavigateType.ManyToOne,
            nameof(SchedName),
            nameof(TriggerName),
            nameof(TriggerGroup))]
        public QrtzTriggers? Trigger { get; set; }
    }
    #endregion

    #region QRTZ_CRON_TRIGGERS（Cron触发器表）
    [SugarTable("QRTZ_CRON_TRIGGERS")]
    public class QrtzCronTriggers
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_NAME", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_GROUP", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerGroup { get; set; }

        [SugarColumn(ColumnName = "CRON_EXPRESSION", Length = 250, IsNullable = true)]
        public string CronExpression { get; set; }

        [SugarColumn(ColumnName = "TIME_ZONE_ID", Length = 80, IsNullable = true)]
        public string? TimeZoneId { get; set; }

        // 导航属性：关联触发器（多对一）
        [Navigate(NavigateType.ManyToOne,
            nameof(SchedName),
            nameof(TriggerName),
            nameof(TriggerGroup))]
        public QrtzTriggers? Trigger { get; set; }
    }
    #endregion

    #region QRTZ_BLOB_TRIGGERS（Blob触发器表）
    [SugarTable("QRTZ_BLOB_TRIGGERS")]
    public class QrtzBlobTriggers
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_NAME", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_GROUP", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerGroup { get; set; }

        [SugarColumn(ColumnName = "BLOB_DATA", IsNullable = true, ColumnDataType = "BLOB")]
        public byte[]? BlobData { get; set; }

        // 导航属性：关联触发器（多对一）
        [Navigate(NavigateType.ManyToOne,
            nameof(SchedName),
            nameof(TriggerName),
            nameof(TriggerGroup))]
        public QrtzTriggers? Trigger { get; set; }
    }
    #endregion

    #region QRTZ_CALENDARS（日历表）
    [SugarTable("QRTZ_CALENDARS")]
    public class QrtzCalendars
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "CALENDAR_NAME", IsPrimaryKey = true, Length = 200, IsNullable = true)]
        public string CalendarName { get; set; }

        [SugarColumn(ColumnName = "CALENDAR", IsNullable = true, ColumnDataType = "BLOB")]
        public byte[] Calendar { get; set; }
    }
    #endregion

    #region QRTZ_PAUSED_TRIGGER_GRPS（暂停的触发器组表）
    [SugarTable("QRTZ_PAUSED_TRIGGER_GRPS")]
    public class QrtzPausedTriggerGrps
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_GROUP", IsPrimaryKey = true, Length = 150, IsNullable = true)]
        public string TriggerGroup { get; set; }
    }
    #endregion

    #region QRTZ_FIRED_TRIGGERS（已触发的触发器表）
    [SugarTable("QRTZ_FIRED_TRIGGERS")]
    public class QrtzFiredTriggers
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "ENTRY_ID", IsPrimaryKey = true, Length = 140, IsNullable = true)]
        public string EntryId { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_NAME", Length = 150, IsNullable = true)]
        public string TriggerName { get; set; }

        [SugarColumn(ColumnName = "TRIGGER_GROUP", Length = 150, IsNullable = true)]
        public string TriggerGroup { get; set; }

        [SugarColumn(ColumnName = "INSTANCE_NAME", Length = 200, IsNullable = true)]
        public string InstanceName { get; set; }

        [SugarColumn(ColumnName = "FIRED_TIME", IsNullable = true)]
        public long FiredTime { get; set; }

        [SugarColumn(ColumnName = "SCHED_TIME", IsNullable = true)]
        public long SchedTime { get; set; }

        [SugarColumn(ColumnName = "PRIORITY", IsNullable = true)]
        public int Priority { get; set; }

        [SugarColumn(ColumnName = "STATE", Length = 16, IsNullable = true)]
        public string State { get; set; }

        [SugarColumn(ColumnName = "JOB_NAME", Length = 150, IsNullable = true)]
        public string? JobName { get; set; }

        [SugarColumn(ColumnName = "JOB_GROUP", Length = 150, IsNullable = true)]
        public string? JobGroup { get; set; }

        [SugarColumn(ColumnName = "IS_NONCONCURRENT", IsNullable = true)]
        public bool? IsNonConcurrent { get; set; }

        [SugarColumn(ColumnName = "REQUESTS_RECOVERY", IsNullable = true)]
        public bool? RequestsRecovery { get; set; }
    }
    #endregion

    #region QRTZ_SCHEDULER_STATE（调度器状态表）
    [SugarTable("QRTZ_SCHEDULER_STATE")]
    public class QrtzSchedulerState
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "INSTANCE_NAME", IsPrimaryKey = true, Length = 200, IsNullable = true)]
        public string InstanceName { get; set; }

        [SugarColumn(ColumnName = "LAST_CHECKIN_TIME", IsNullable = true)]
        public long LastCheckinTime { get; set; }

        [SugarColumn(ColumnName = "CHECKIN_INTERVAL", IsNullable = true)]
        public long CheckinInterval { get; set; }
    }
    #endregion

    #region QRTZ_LOCKS（锁表）
    [SugarTable("QRTZ_LOCKS")]
    public class QrtzLocks
    {
        [SugarColumn(ColumnName = "SCHED_NAME", IsPrimaryKey = true, Length = 120, IsNullable = true)]
        public string SchedName { get; set; }

        [SugarColumn(ColumnName = "LOCK_NAME", IsPrimaryKey = true, Length = 40, IsNullable = true)]
        public string LockName { get; set; }
    }
    #endregion
}
