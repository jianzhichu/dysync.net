using dy.net.model.dto;
using Serilog;
using SqlSugar;
using System.Data.Common;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace dy.net.service
{
    /// <summary>
    /// Resolves the active database before DI is built and persists a successful migration.
    /// The separate file avoids rewriting a deployment-owned appsettings.json.
    /// </summary>
    public class DatabaseConfigurationService
    {
        private const string SettingsFileName = "database.json";
        private readonly string _databaseDirectory;
        private readonly IConfiguration _configuration;

        public DatabaseConfigurationService(IConfiguration configuration, string dbPath)
        {
            _configuration = configuration;
            _databaseDirectory = string.IsNullOrWhiteSpace(dbPath)
                ? Path.Combine(Environment.CurrentDirectory, "db")
                : Path.Combine(dbPath, "db");
        }

        public string SettingsFilePath => Path.Combine(_databaseDirectory, SettingsFileName);

        public bool HasPersistedSelection => File.Exists(SettingsFilePath);

        public DatabaseSettings GetActiveSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    var persisted = JsonSerializer.Deserialize<DatabaseSettings>(
                        File.ReadAllText(SettingsFilePath), JsonOptions());
                    if (persisted != null && DatabaseKinds.IsSupported(persisted.DbType))
                    {
                        persisted.DbType = DatabaseKinds.Normalize(persisted.DbType);
                        Validate(persisted, allowSqliteWithoutConnectionString: true);
                        return persisted;
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"数据库配置文件无效：{SettingsFilePath}", ex);
                }
            }

            var fallback = new DatabaseSettings
            {
                DbType = DatabaseKinds.Normalize(_configuration["dbtype"]),
                ConnectionString = _configuration["dbconn"] ?? string.Empty
            };

            if (fallback.DbType == DatabaseKinds.Sqlite && string.IsNullOrWhiteSpace(fallback.ConnectionString))
            {
                fallback.ConnectionString = CreateSqliteConnectionString();
            }

            Validate(fallback, allowSqliteWithoutConnectionString: false);
            return fallback;
        }

        public DatabaseSettings CreateTargetSettings(string dbType, string connectionString)
        {
            var settings = new DatabaseSettings
            {
                DbType = DatabaseKinds.Normalize(dbType),
                ConnectionString = connectionString?.Trim() ?? string.Empty
            };
            Validate(settings, allowSqliteWithoutConnectionString: false);
            return settings;
        }

        public DatabaseSettings CreateTargetSettings(DatabaseMigrationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("数据库配置不能为空");
            }

            if (!DatabaseKinds.IsSupported(request.DbType))
            {
                throw new ArgumentException("迁移目标仅支持 Sqlite、MySql 和 PostgreSql");
            }

            if (!string.IsNullOrWhiteSpace(request.ConnectionString))
            {
                return CreateTargetSettings(request.DbType, request.ConnectionString);
            }

            var normalizedType = DatabaseKinds.Normalize(request.DbType);
            if (normalizedType == DatabaseKinds.Sqlite)
            {
                return CreateTargetSettings(
                    DatabaseKinds.Sqlite,
                    CreateSqliteMigrationConnectionString());
            }
            if (string.IsNullOrWhiteSpace(request.Host) ||
                string.IsNullOrWhiteSpace(request.UserName) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Host、账号和密码不能为空");
            }

            var port = request.Port ??
                (normalizedType == DatabaseKinds.MySql ? 3306 : 5432);
            if (port <= 0 || port > 65535)
            {
                throw new ArgumentException("数据库端口必须在 1-65535 之间");
            }

            var databaseName = string.IsNullOrWhiteSpace(request.DatabaseName)
                ? "dysync"
                : request.DatabaseName.Trim();
            if (databaseName.Length > 63 ||
                !Regex.IsMatch(databaseName, "^[A-Za-z0-9_-]+$"))
            {
                throw new ArgumentException("数据库名只能包含字母、数字、下划线和短横线，且不能超过 63 个字符");
            }
            var builder = new DbConnectionStringBuilder();
            if (normalizedType == DatabaseKinds.MySql)
            {
                builder["Server"] = request.Host.Trim();
                builder["Port"] = port;
                builder["Database"] = databaseName;
                builder["Uid"] = request.UserName.Trim();
                builder["Pwd"] = request.Password;
                builder["Charset"] = "utf8mb4";
            }
            else
            {
                builder["Host"] = request.Host.Trim();
                builder["Port"] = port;
                builder["Database"] = databaseName;
                builder["Username"] = request.UserName.Trim();
                builder["Password"] = request.Password;
            }

            return CreateTargetSettings(normalizedType, builder.ConnectionString);
        }

        public void Save(DatabaseSettings settings)
        {
            Validate(settings, allowSqliteWithoutConnectionString: false);
            Directory.CreateDirectory(_databaseDirectory);

            var tempPath = SettingsFilePath + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(settings, JsonOptions()));
            File.Move(tempPath, SettingsFilePath, true);
            Log.Information("数据库连接类型已固定为 {DbType}，配置写入 {SettingsFilePath}",
                settings.DbType, SettingsFilePath);
        }

        public ConnectionConfig CreateConnectionConfig(DatabaseSettings settings)
        {
            return new ConnectionConfig
            {
                ConnectionString = settings.ConnectionString,
                InitKeyType = InitKeyType.Attribute,
                DbType = ToSqlSugarDbType(settings.DbType),
                IsAutoCloseConnection = true,
                MoreSettings = new ConnMoreSettings
                {
                    IsAutoRemoveDataCache = true
                }
            };
        }

        public string CreateSqliteConnectionString()
        {
            Directory.CreateDirectory(_databaseDirectory);
            var dbFilePath = Path.Combine(_databaseDirectory, "dy.sqlite");
            return $"DataSource={dbFilePath}";
        }

        /// <summary>
        /// Creates a new SQLite target path for a database migration. A fresh file is used
        /// so an old dy.sqlite left behind by an earlier migration is never overwritten.
        /// </summary>
        public string CreateSqliteMigrationConnectionString()
        {
            Directory.CreateDirectory(_databaseDirectory);
            var fileName = $"dy-{DateTime.UtcNow:yyyyMMddHHmmssfff}.sqlite";
            return $"DataSource={Path.Combine(_databaseDirectory, fileName)}";
        }

        public static DbType ToSqlSugarDbType(string dbType) =>
            DatabaseKinds.Normalize(dbType) switch
            {
                DatabaseKinds.MySql => DbType.MySql,
                DatabaseKinds.PostgreSql => DbType.PostgreSQL,
                _ => DbType.Sqlite
            };

        private static void Validate(DatabaseSettings settings, bool allowSqliteWithoutConnectionString)
        {
            if (settings == null || !DatabaseKinds.IsSupported(settings.DbType))
            {
                throw new ArgumentException("仅支持 Sqlite、MySql 和 PostgreSql 数据库");
            }

            if ((!allowSqliteWithoutConnectionString || settings.DbType != DatabaseKinds.Sqlite) &&
                string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ArgumentException("数据库连接字符串不能为空");
            }
        }

        private static JsonSerializerOptions JsonOptions() => new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }
}
