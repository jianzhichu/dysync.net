using dy.net.model.dto;
using dy.net.model.entity;
using Serilog;
using SqlSugar;
using System.Reflection;

namespace dy.net.service
{
    public class DatabaseMigrationService
    {
        private const int BatchSize = 1000;
        private static readonly SemaphoreSlim MigrationLock = new(1, 1);
        private static readonly Type[] BusinessEntityTypes = BusinessEntityRegistry.Types;

        private readonly ISqlSugarClient _source;
        private readonly DatabaseConfigurationService _configurationService;

        public DatabaseMigrationService(
            ISqlSugarClient source,
            DatabaseConfigurationService configurationService)
        {
            _source = source;
            _configurationService = configurationService;
        }

        public DatabaseStatusDto GetStatus()
        {
            var settings = _configurationService.GetActiveSettings();
            return new DatabaseStatusDto
            {
                DbType = settings.DbType,
                CanMigrate = true,
                IsExternal = settings.DbType != DatabaseKinds.Sqlite,
                RequiresSelection = settings.DbType == DatabaseKinds.Sqlite &&
                    !_configurationService.HasPersistedSelection
            };
        }

        public void ConfirmSqliteSelection()
        {
            var current = _configurationService.GetActiveSettings();
            if (current.DbType != DatabaseKinds.Sqlite)
            {
                throw new InvalidOperationException("当前已经固定使用外部数据库");
            }

            _configurationService.Save(current);
        }

        public async Task<DatabaseMigrationResult> MigrateAsync(
            DatabaseMigrationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!await MigrationLock.WaitAsync(0, cancellationToken))
            {
                throw new InvalidOperationException("已有数据库迁移任务正在执行，请勿重复提交");
            }

            try
            {
                var current = _configurationService.GetActiveSettings();
                var targetSettings = _configurationService.CreateTargetSettings(request);
                using var target = new SqlSugarClient(
                    _configurationService.CreateConnectionConfig(targetSettings));

                try
                {
                    // SqlSugar creates the requested database when it does not exist and
                    // the supplied account has CREATE DATABASE permission.
                    target.DbMaintenance.CreateDatabase();
                    target.Ado.CheckConnection();
                }
                catch (Exception ex)
                {
                    var message = targetSettings.DbType == DatabaseKinds.Sqlite
                        ? "无法创建或连接目标 SQLite 数据库，请检查持久化目录的读写权限"
                        : "无法创建或连接目标数据库，请检查数据库名、网络及账号的建库/建表权限";
                    throw new InvalidOperationException(message, ex);
                }

                Log.Information("开始从 {SourceDbType} 迁移到 {TargetDbType}，共 {TableCount} 张业务表",
                    current.DbType, targetSettings.DbType, BusinessEntityTypes.Length);

                target.CodeFirst.InitTables(BusinessEntityTypes);
                await EnsureTargetIsEmptyAsync(target);

                long migratedRows = 0;
                target.Ado.BeginTran();
                try
                {
                    foreach (var entityType in BusinessEntityTypes)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        migratedRows += await InvokeMigrateEntityAsync(entityType, target, cancellationToken);
                    }
                    target.Ado.CommitTran();
                }
                catch
                {
                    target.Ado.RollbackTran();
                    throw;
                }

                _configurationService.Save(targetSettings);
                Log.Information("数据库迁移完成，共迁移 {RowCount} 行", migratedRows);

                return new DatabaseMigrationResult
                {
                    DbType = targetSettings.DbType,
                    TableCount = BusinessEntityTypes.Length,
                    RowCount = migratedRows,
                    Restarting = true
                };
            }
            finally
            {
                MigrationLock.Release();
            }
        }

        private static async Task EnsureTargetIsEmptyAsync(ISqlSugarClient target)
        {
            foreach (var entityType in BusinessEntityTypes)
            {
                var method = typeof(DatabaseMigrationService)
                    .GetMethod(nameof(HasAnyAsync), BindingFlags.NonPublic | BindingFlags.Static)
                    ?.MakeGenericMethod(entityType);
                var task = (Task<bool>)method.Invoke(null, new object[] { target });
                if (await task)
                {
                    var tableName = entityType.GetCustomAttribute<SugarTable>()?.TableName ?? entityType.Name;
                    throw new InvalidOperationException($"目标数据库表 {tableName} 已有数据，为防止覆盖已取消迁移");
                }
            }
        }

        private static Task<bool> HasAnyAsync<TEntity>(ISqlSugarClient target)
            where TEntity : class, new() => target.Queryable<TEntity>().AnyAsync();

        private async Task<int> InvokeMigrateEntityAsync(
            Type entityType,
            ISqlSugarClient target,
            CancellationToken cancellationToken)
        {
            var method = typeof(DatabaseMigrationService)
                .GetMethod(nameof(MigrateEntityAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                ?.MakeGenericMethod(entityType);
            var task = (Task<int>)method.Invoke(this, new object[] { target, cancellationToken });
            return await task;
        }

        private async Task<int> MigrateEntityAsync<TEntity>(
            ISqlSugarClient target,
            CancellationToken cancellationToken)
            where TEntity : class, new()
        {
            var total = await _source.Queryable<TEntity>().CountAsync();
            var migrated = 0;

            while (migrated < total)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rows = await _source.Queryable<TEntity>()
                    .Skip(migrated)
                    .Take(Math.Min(BatchSize, total - migrated))
                    .ToListAsync();
                if (rows.Count == 0) break;

                await target.Insertable(rows).ExecuteCommandAsync();
                migrated += rows.Count;
            }

            var tableName = typeof(TEntity).GetCustomAttribute<SugarTable>()?.TableName
                ?? typeof(TEntity).Name;
            Log.Information("迁移数据表 {TableName}：{RowCount} 行", tableName, migrated);
            return migrated;
        }
    }
}
