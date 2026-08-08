using dy.net.model.entity;

namespace dy.net.model.dto
{
    public static class DatabaseKinds
    {
        public const string Sqlite = "Sqlite";
        public const string MySql = "MySql";
        public const string PostgreSql = "PostgreSql";

        public static bool IsSupported(string value) =>
            string.Equals(value, Sqlite, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, MySql, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, PostgreSql, StringComparison.OrdinalIgnoreCase);

        public static string Normalize(string value)
        {
            if (string.Equals(value, MySql, StringComparison.OrdinalIgnoreCase)) return MySql;
            if (string.Equals(value, PostgreSql, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "Postgres", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "PgSql", StringComparison.OrdinalIgnoreCase)) return PostgreSql;
            return Sqlite;
        }
    }

    public class DatabaseSettings
    {
        public string DbType { get; set; } = DatabaseKinds.Sqlite;
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class DatabaseMigrationRequest
    {
        public string DbType { get; set; }
        public string ConnectionString { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; }
    }

    public class DatabaseStatusDto
    {
        public string DbType { get; set; }
        public bool CanMigrate { get; set; }
        public bool IsExternal { get; set; }
        public bool RequiresSelection { get; set; }
    }

    public class DatabaseMigrationResult
    {
        public string DbType { get; set; }
        public int TableCount { get; set; }
        public long RowCount { get; set; }
        public bool Restarting { get; set; }
    }

    public static class DatabaseMigrationJobStates
    {
        public const string Idle = "Idle";
        public const string Queued = "Queued";
        public const string Running = "Running";
        public const string Succeeded = "Succeeded";
        public const string Failed = "Failed";
    }

    public class DatabaseMigrationJobStatusDto
    {
        public string JobId { get; set; } = string.Empty;
        public string State { get; set; } = DatabaseMigrationJobStates.Idle;
        public string SourceDbType { get; set; } = string.Empty;
        public string TargetDbType { get; set; } = string.Empty;
        public string CurrentTable { get; set; } = string.Empty;
        public int TableCount { get; set; }
        public int CompletedTables { get; set; }
        public long TotalRows { get; set; }
        public long MigratedRows { get; set; }
        public decimal ProgressPercent { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public bool Restarting { get; set; }
        public int RefreshIntervalSeconds { get; set; } = 5;
        public List<DatabaseMigrationHistoryItemDto> History { get; set; } = new();
    }

    public class DatabaseMigrationHistoryItemDto
    {
        public string JobId { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string SourceDbType { get; set; } = string.Empty;
        public string TargetDbType { get; set; } = string.Empty;
        public int TableCount { get; set; }
        public long TotalRows { get; set; }
        public long MigratedRows { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class DatabaseMigrationProgress
    {
        public string SourceDbType { get; set; } = string.Empty;
        public string TargetDbType { get; set; } = string.Empty;
        public string CurrentTable { get; set; } = string.Empty;
        public int TableCount { get; set; }
        public int CompletedTables { get; set; }
        public long TotalRows { get; set; }
        public long MigratedRows { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class DeskInitRequest : DouyinCookie
    {
        public string DatabaseType { get; set; } = DatabaseKinds.Sqlite;
        public string DatabaseConnectionString { get; set; } = string.Empty;
        public string DatabaseHost { get; set; } = string.Empty;
        public int? DatabasePort { get; set; }
        public string DatabaseUserName { get; set; } = string.Empty;
        public string DatabasePassword { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}
