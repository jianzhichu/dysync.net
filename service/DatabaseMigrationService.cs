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
            CancellationToken cancellationToken = default,
            Action<DatabaseMigrationProgress> reportProgress = null)
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

                _configurationService.EnsureDatabaseAvailable(target, targetSettings);

                Log.Information("开始从 {SourceDbType} 迁移到 {TargetDbType}，共 {TableCount} 张业务表",
                    current.DbType, targetSettings.DbType, BusinessEntityTypes.Length);

                reportProgress?.Invoke(new DatabaseMigrationProgress
                {
                    SourceDbType = current.DbType,
                    TargetDbType = targetSettings.DbType,
                    TableCount = BusinessEntityTypes.Length,
                    Message = "正在创建并检查目标数据库"
                });

                target.CodeFirst.InitTables(BusinessEntityTypes);
                await EnsureTargetIsEmptyAsync(target);

                var tableTotals = new Dictionary<Type, int>();
                long totalRows = 0;
                foreach (var entityType in BusinessEntityTypes)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var count = await InvokeCountEntityAsync(entityType);
                    tableTotals[entityType] = count;
                    totalRows += count;
                }

                long migratedRows = 0;
                target.Ado.BeginTran();
                try
                {
                    for (var tableIndex = 0; tableIndex < BusinessEntityTypes.Length; tableIndex++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var entityType = BusinessEntityTypes[tableIndex];
                        migratedRows += await InvokeMigrateEntityAsync(
                            entityType,
                            target,
                            cancellationToken,
                            tableTotals[entityType],
                            migratedRows,
                            totalRows,
                            tableIndex,
                            current.DbType,
                            targetSettings.DbType,
                            reportProgress);
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

        private async Task<int> InvokeCountEntityAsync(Type entityType)
        {
            var method = typeof(DatabaseMigrationService)
                .GetMethod(nameof(CountEntityAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                ?.MakeGenericMethod(entityType);
            var task = (Task<int>)method.Invoke(this, Array.Empty<object>());
            return await task;
        }

        private Task<int> CountEntityAsync<TEntity>() where TEntity : class, new() =>
            _source.Queryable<TEntity>().CountAsync();

        private async Task<int> InvokeMigrateEntityAsync(
            Type entityType,
            ISqlSugarClient target,
            CancellationToken cancellationToken,
            int total,
            long migratedBeforeTable,
            long totalRows,
            int tableIndex,
            string sourceDbType,
            string targetDbType,
            Action<DatabaseMigrationProgress> reportProgress)
        {
            var method = typeof(DatabaseMigrationService)
                .GetMethod(nameof(MigrateEntityAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                ?.MakeGenericMethod(entityType);
            var task = (Task<int>)method.Invoke(this, new object[]
            {
                target, cancellationToken, total, migratedBeforeTable, totalRows,
                tableIndex, sourceDbType, targetDbType, reportProgress
            });
            return await task;
        }

        private async Task<int> MigrateEntityAsync<TEntity>(
            ISqlSugarClient target,
            CancellationToken cancellationToken,
            int total,
            long migratedBeforeTable,
            long totalRows,
            int tableIndex,
            string sourceDbType,
            string targetDbType,
            Action<DatabaseMigrationProgress> reportProgress)
            where TEntity : class, new()
        {
            var migrated = 0;
            var tableName = typeof(TEntity).GetCustomAttribute<SugarTable>()?.TableName
                ?? typeof(TEntity).Name;

            ReportProgress("正在迁移数据", migrated);

            while (migrated < total)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rows = await _source.Queryable<TEntity>()
                    .Skip(migrated)
                    .Take(Math.Min(BatchSize, total - migrated))
                    .ToListAsync();
                if (rows.Count == 0) break;

                try
                {
                    await target.Insertable(rows).ExecuteCommandAsync();
                }
                catch (Exception ex)
                {
                    var firstRow = migrated + 1;
                    var lastRow = migrated + rows.Count;
                    throw new InvalidOperationException(
                        $"迁移表 {tableName} 第 {firstRow}-{lastRow} 行失败：{ex.GetBaseException().Message}", ex);
                }
                migrated += rows.Count;
                ReportProgress("正在迁移数据", migrated);
            }

            Log.Information("迁移数据表 {TableName}：{RowCount} 行", tableName, migrated);
            return migrated;

            void ReportProgress(string message, int tableMigrated)
            {
                reportProgress?.Invoke(new DatabaseMigrationProgress
                {
                    SourceDbType = sourceDbType,
                    TargetDbType = targetDbType,
                    CurrentTable = tableName,
                    TableCount = BusinessEntityTypes.Length,
                    CompletedTables = tableIndex + (tableMigrated >= total ? 1 : 0),
                    TotalRows = totalRows,
                    MigratedRows = migratedBeforeTable + tableMigrated,
                    Message = message
                });
            }
        }
    }
}
