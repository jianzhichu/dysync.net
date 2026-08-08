using SqlSugar;

namespace dy.net.service
{
    /// <summary>
    /// Creates the Quartz.NET 3.x ADO job-store schema without copying runtime state.
    /// The column set matches Quartz's official MySQL/PostgreSQL/SQLite table scripts.
    /// </summary>
    public static class QuartzSchemaInitializer
    {
        public static void EnsureCreated(ISqlSugarClient db)
        {
            var dbType = db.CurrentConnectionConfig.DbType;
            var isMySql = dbType == DbType.MySql;
            var isPostgres = dbType == DbType.PostgreSQL;
            var nameType = isMySql ? "VARCHAR(200)" : "TEXT";
            var shortNameType = isMySql ? "VARCHAR(120)" : "TEXT";
            var descriptionType = isMySql ? "VARCHAR(250)" : "TEXT";
            var stateType = isMySql ? "VARCHAR(16)" : "TEXT";
            var triggerType = isMySql ? "VARCHAR(8)" : "TEXT";
            var timeZoneType = isMySql ? "VARCHAR(80)" : "TEXT";
            var propertyType = isMySql ? "VARCHAR(512)" : "TEXT";
            var entryType = isMySql ? "VARCHAR(140)" : "TEXT";
            var lockType = isMySql ? "VARCHAR(40)" : "TEXT";
            var blobType = isPostgres ? "BYTEA" : "BLOB";
            var boolType = isPostgres ? "BOOL" : "BOOLEAN";
            var tableSuffix = isMySql ? " ENGINE=InnoDB" : string.Empty;

            string[] tableSql =
            {
                $@"CREATE TABLE IF NOT EXISTS QRTZ_JOB_DETAILS (
                    SCHED_NAME {shortNameType} NOT NULL, JOB_NAME {nameType} NOT NULL,
                    JOB_GROUP {nameType} NOT NULL, DESCRIPTION {descriptionType} NULL,
                    JOB_CLASS_NAME {descriptionType} NOT NULL, IS_DURABLE {boolType} NOT NULL,
                    IS_NONCONCURRENT {boolType} NOT NULL, IS_UPDATE_DATA {boolType} NOT NULL,
                    REQUESTS_RECOVERY {boolType} NOT NULL, JOB_DATA {blobType} NULL,
                    PRIMARY KEY (SCHED_NAME, JOB_NAME, JOB_GROUP)){tableSuffix}",

                $@"CREATE TABLE IF NOT EXISTS QRTZ_TRIGGERS (
                    SCHED_NAME {shortNameType} NOT NULL, TRIGGER_NAME {nameType} NOT NULL,
                    TRIGGER_GROUP {nameType} NOT NULL, JOB_NAME {nameType} NOT NULL,
                    JOB_GROUP {nameType} NOT NULL, DESCRIPTION {descriptionType} NULL,
                    NEXT_FIRE_TIME BIGINT NULL, PREV_FIRE_TIME BIGINT NULL, PRIORITY INTEGER NULL,
                    TRIGGER_STATE {stateType} NOT NULL, TRIGGER_TYPE {triggerType} NOT NULL,
                    START_TIME BIGINT NOT NULL, END_TIME BIGINT NULL, CALENDAR_NAME {nameType} NULL,
                    MISFIRE_INSTR SMALLINT NULL, JOB_DATA {blobType} NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP),
                    FOREIGN KEY (SCHED_NAME, JOB_NAME, JOB_GROUP)
                      REFERENCES QRTZ_JOB_DETAILS (SCHED_NAME, JOB_NAME, JOB_GROUP)){tableSuffix}",

                $@"CREATE TABLE IF NOT EXISTS QRTZ_SIMPLE_TRIGGERS (
                    SCHED_NAME {shortNameType} NOT NULL, TRIGGER_NAME {nameType} NOT NULL,
                    TRIGGER_GROUP {nameType} NOT NULL, REPEAT_COUNT BIGINT NOT NULL,
                    REPEAT_INTERVAL BIGINT NOT NULL, TIMES_TRIGGERED BIGINT NOT NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP),
                    FOREIGN KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                      REFERENCES QRTZ_TRIGGERS (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                      ON DELETE CASCADE){tableSuffix}",

                $@"CREATE TABLE IF NOT EXISTS QRTZ_SIMPROP_TRIGGERS (
                    SCHED_NAME {shortNameType} NOT NULL, TRIGGER_NAME {nameType} NOT NULL,
                    TRIGGER_GROUP {nameType} NOT NULL, STR_PROP_1 {propertyType} NULL,
                    STR_PROP_2 {propertyType} NULL, STR_PROP_3 {propertyType} NULL,
                    INT_PROP_1 INTEGER NULL, INT_PROP_2 INTEGER NULL,
                    LONG_PROP_1 BIGINT NULL, LONG_PROP_2 BIGINT NULL,
                    DEC_PROP_1 NUMERIC(13,4) NULL, DEC_PROP_2 NUMERIC(13,4) NULL,
                    BOOL_PROP_1 {boolType} NULL, BOOL_PROP_2 {boolType} NULL,
                    TIME_ZONE_ID {timeZoneType} NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP),
                    FOREIGN KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                      REFERENCES QRTZ_TRIGGERS (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                      ON DELETE CASCADE){tableSuffix}",

                $@"CREATE TABLE IF NOT EXISTS QRTZ_CRON_TRIGGERS (
                    SCHED_NAME {shortNameType} NOT NULL, TRIGGER_NAME {nameType} NOT NULL,
                    TRIGGER_GROUP {nameType} NOT NULL, CRON_EXPRESSION {nameType} NOT NULL,
                    TIME_ZONE_ID {timeZoneType} NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP),
                    FOREIGN KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                      REFERENCES QRTZ_TRIGGERS (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                      ON DELETE CASCADE){tableSuffix}",

                $@"CREATE TABLE IF NOT EXISTS QRTZ_BLOB_TRIGGERS (
                    SCHED_NAME {shortNameType} NOT NULL, TRIGGER_NAME {nameType} NOT NULL,
                    TRIGGER_GROUP {nameType} NOT NULL, BLOB_DATA {blobType} NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP),
                    FOREIGN KEY (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                      REFERENCES QRTZ_TRIGGERS (SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP)
                      ON DELETE CASCADE){tableSuffix}",

                $@"CREATE TABLE IF NOT EXISTS QRTZ_CALENDARS (
                    SCHED_NAME {shortNameType} NOT NULL, CALENDAR_NAME {nameType} NOT NULL,
                    CALENDAR {blobType} NOT NULL,
                    PRIMARY KEY (SCHED_NAME, CALENDAR_NAME)){tableSuffix}",

                $@"CREATE TABLE IF NOT EXISTS QRTZ_PAUSED_TRIGGER_GRPS (
                    SCHED_NAME {shortNameType} NOT NULL, TRIGGER_GROUP {nameType} NOT NULL,
                    PRIMARY KEY (SCHED_NAME, TRIGGER_GROUP)){tableSuffix}",

                $@"CREATE TABLE IF NOT EXISTS QRTZ_FIRED_TRIGGERS (
                    SCHED_NAME {shortNameType} NOT NULL, ENTRY_ID {entryType} NOT NULL,
                    TRIGGER_NAME {nameType} NOT NULL, TRIGGER_GROUP {nameType} NOT NULL,
                    INSTANCE_NAME {nameType} NOT NULL, FIRED_TIME BIGINT NOT NULL,
                    SCHED_TIME BIGINT NOT NULL, PRIORITY INTEGER NOT NULL,
                    STATE {stateType} NOT NULL, JOB_NAME {nameType} NULL, JOB_GROUP {nameType} NULL,
                    IS_NONCONCURRENT {boolType} NULL, REQUESTS_RECOVERY {boolType} NULL,
                    PRIMARY KEY (SCHED_NAME, ENTRY_ID)){tableSuffix}",

                $@"CREATE TABLE IF NOT EXISTS QRTZ_SCHEDULER_STATE (
                    SCHED_NAME {shortNameType} NOT NULL, INSTANCE_NAME {nameType} NOT NULL,
                    LAST_CHECKIN_TIME BIGINT NOT NULL, CHECKIN_INTERVAL BIGINT NOT NULL,
                    PRIMARY KEY (SCHED_NAME, INSTANCE_NAME)){tableSuffix}",

                $@"CREATE TABLE IF NOT EXISTS QRTZ_LOCKS (
                    SCHED_NAME {shortNameType} NOT NULL, LOCK_NAME {lockType} NOT NULL,
                    PRIMARY KEY (SCHED_NAME, LOCK_NAME)){tableSuffix}"
            };

            foreach (var sql in tableSql)
            {
                db.Ado.ExecuteCommand(sql);
            }

            EnsureIndexes(db, isMySql);
        }

        private static void EnsureIndexes(ISqlSugarClient db, bool isMySql)
        {
            var ifNotExists = isMySql ? string.Empty : "IF NOT EXISTS ";
            (string Name, string Table, string Definition)[] indexes =
            {
                ("IDX_QRTZ_J_REQ_RECOVERY", "QRTZ_JOB_DETAILS", "ON QRTZ_JOB_DETAILS (SCHED_NAME, REQUESTS_RECOVERY)"),
                ("IDX_QRTZ_J_GRP", "QRTZ_JOB_DETAILS", "ON QRTZ_JOB_DETAILS (SCHED_NAME, JOB_GROUP)"),
                ("IDX_QRTZ_T_J", "QRTZ_TRIGGERS", "ON QRTZ_TRIGGERS (SCHED_NAME, JOB_NAME, JOB_GROUP)"),
                ("IDX_QRTZ_T_STATE", "QRTZ_TRIGGERS", "ON QRTZ_TRIGGERS (SCHED_NAME, TRIGGER_STATE)"),
                ("IDX_QRTZ_T_NEXT_FIRE_TIME", "QRTZ_TRIGGERS", "ON QRTZ_TRIGGERS (SCHED_NAME, NEXT_FIRE_TIME)"),
                ("IDX_QRTZ_T_NFT_ST", "QRTZ_TRIGGERS", "ON QRTZ_TRIGGERS (SCHED_NAME, TRIGGER_STATE, NEXT_FIRE_TIME)"),
                ("IDX_QRTZ_T_NFT_MISFIRE", "QRTZ_TRIGGERS", "ON QRTZ_TRIGGERS (SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME)"),
                ("IDX_QRTZ_FT_TRIG_INST_NAME", "QRTZ_FIRED_TRIGGERS", "ON QRTZ_FIRED_TRIGGERS (SCHED_NAME, INSTANCE_NAME)"),
                ("IDX_QRTZ_FT_J_G", "QRTZ_FIRED_TRIGGERS", "ON QRTZ_FIRED_TRIGGERS (SCHED_NAME, JOB_NAME, JOB_GROUP)")
            };

            foreach (var index in indexes)
            {
                if (isMySql &&
                    DatabaseIndexHelper.MySqlIndexExists(db, index.Table, index.Name))
                {
                    continue;
                }

                db.Ado.ExecuteCommand(
                    $"CREATE INDEX {ifNotExists}{index.Name} {index.Definition}");
            }
        }
    }
}
