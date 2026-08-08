using dy.net.model.dto;
using Quartz;
using Serilog;
using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Channels;

namespace dy.net.service
{
    /// <summary>
    /// Runs the single allowed database migration independently of the HTTP request.
    /// The public status is persisted so it survives the required application restart.
    /// </summary>
    public class DatabaseMigrationJobService : BackgroundService
    {
        public const int RefreshIntervalSeconds = 5;

        private readonly Channel<DatabaseMigrationRequest> _queue =
            Channel.CreateBounded<DatabaseMigrationRequest>(new BoundedChannelOptions(1)
            {
                SingleReader = true,
                SingleWriter = false
            });
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DatabaseConfigurationService _configurationService;
        private readonly ApplicationRestartService _restartService;
        private readonly ConcurrentDictionary<string, DateTime> _lastStatusRequests = new();
        private readonly object _statusLock = new();
        private DatabaseMigrationJobStatusDto _status;
        private List<DatabaseMigrationHistoryItemDto> _history;
        private DateTime _lastPersistedAt = DateTime.MinValue;
        private bool _active;
        private bool _restartScheduled;

        public DatabaseMigrationJobService(
            IServiceScopeFactory scopeFactory,
            DatabaseConfigurationService configurationService,
            ApplicationRestartService restartService)
        {
            _scopeFactory = scopeFactory;
            _configurationService = configurationService;
            _restartService = restartService;
            _status = LoadStatus();
            _history = LoadHistory();

            if (_status.State == DatabaseMigrationJobStates.Queued ||
                _status.State == DatabaseMigrationJobStates.Running)
            {
                _status.State = DatabaseMigrationJobStates.Failed;
                _status.Message = "服务重启，上一迁移任务已中断";
                _status.CompletedAt = DateTime.UtcNow;
                _status.UpdatedAt = DateTime.UtcNow;
                PersistStatus(force: true);
                AddHistory();
            }
            else if (_status.State == DatabaseMigrationJobStates.Succeeded &&
                _status.Restarting)
            {
                // Reaching this constructor proves the replacement process is running.
                _status.Restarting = false;
                _status.Message = $"迁移完成，共迁移 {_status.MigratedRows} 行，服务已重启";
                _status.UpdatedAt = DateTime.UtcNow;
                PersistStatus(force: true);
                AddHistory();
            }
        }

        public DatabaseMigrationJobStatusDto Start(DatabaseMigrationRequest request)
        {
            // Validate the shape before accepting the background task. Connection and
            // empty-target checks are intentionally performed by the worker.
            _configurationService.CreateTargetSettings(request);

            lock (_statusLock)
            {
                if (_active || _restartScheduled)
                {
                    throw new InvalidOperationException(_restartScheduled
                        ? "数据库已迁移成功，服务正在重启"
                        : "已有数据库迁移任务正在执行，请勿重复提交");
                }

                var now = DateTime.UtcNow;
                _status = new DatabaseMigrationJobStatusDto
                {
                    JobId = Guid.NewGuid().ToString("N"),
                    State = DatabaseMigrationJobStates.Queued,
                    SourceDbType = _configurationService.GetActiveSettings().DbType,
                    TargetDbType = DatabaseKinds.Normalize(request.DbType),
                    Message = "迁移任务已提交，等待后台执行",
                    UpdatedAt = now,
                    RefreshIntervalSeconds = RefreshIntervalSeconds
                };
                _active = true;

                if (!_queue.Writer.TryWrite(CloneRequest(request)))
                {
                    _active = false;
                    throw new InvalidOperationException("迁移任务队列繁忙，请稍后重试");
                }

                PersistStatus(force: true);
                var result = CloneStatus(_status);
                result.History = _history.Select(CloneHistoryItem).ToList();
                return result;
            }
        }

        public DatabaseMigrationJobStatusDto GetStatus() 
        {
            lock (_statusLock)
            {
                var result = CloneStatus(_status);
                result.History = _history.Select(CloneHistoryItem).ToList();
                return result;
            }
        }

        public bool TryGetStatus(string clientKey, out DatabaseMigrationJobStatusDto status, out int retryAfterSeconds)
        {
            var now = DateTime.UtcNow;
            if (_lastStatusRequests.TryGetValue(clientKey, out var lastRequest))
            {
                var remaining = TimeSpan.FromSeconds(RefreshIntervalSeconds) - (now - lastRequest);
                if (remaining > TimeSpan.Zero)
                {
                    status = null;
                    retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));
                    return false;
                }
            }

            _lastStatusRequests[clientKey] = now;
            if (_lastStatusRequests.Count > 1000)
            {
                foreach (var stale in _lastStatusRequests.Where(x => now - x.Value > TimeSpan.FromMinutes(10)))
                {
                    _lastStatusRequests.TryRemove(stale.Key, out _);
                }
            }

            status = GetStatus();
            retryAfterSeconds = 0;
            return true;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var request in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                await RunMigrationAsync(request, stoppingToken);
            }
        }

        private async Task RunMigrationAsync(
            DatabaseMigrationRequest request,
            CancellationToken stoppingToken)
        {
            IScheduler scheduler = null;
            var schedulerPaused = false;
            var migrationSucceeded = false;

            UpdateStatus(status =>
            {
                status.State = DatabaseMigrationJobStates.Running;
                status.StartedAt = DateTime.UtcNow;
                status.Message = "后台迁移已开始";
            }, forcePersist: true);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
                scheduler = await schedulerFactory.GetScheduler();

                UpdateStatus(status =>
                {
                    status.Message = "正在暂停同步任务";
                    status.CurrentTable = string.Empty;
                }, forcePersist: true);

                await scheduler.Standby();
                schedulerPaused = true;
                await WaitForRunningJobsAsync(scheduler, stoppingToken);

                var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
                var result = await migrationService.MigrateAsync(
                    request,
                    stoppingToken,
                    progress => UpdateProgress(progress));

                UpdateStatus(status =>
                {
                    status.State = DatabaseMigrationJobStates.Succeeded;
                    status.CompletedTables = result.TableCount;
                    status.TableCount = result.TableCount;
                    status.MigratedRows = result.RowCount;
                    status.TotalRows = Math.Max(status.TotalRows, result.RowCount);
                    status.ProgressPercent = 100;
                    status.Message = $"迁移完成，共迁移 {result.RowCount} 行，服务正在重启";
                    status.CompletedAt = DateTime.UtcNow;
                    status.Restarting = true;
                }, forcePersist: true);
                AddHistory();
                migrationSucceeded = true;

                lock (_statusLock)
                {
                    _restartScheduled = true;
                }
                _restartService.RestartAfterResponse(TimeSpan.FromSeconds(3));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                MarkFailed("服务停止，迁移任务已取消");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "后台数据库迁移失败");
                MarkFailed(ex.GetBaseException().Message);
            }
            finally
            {
                if (schedulerPaused && !migrationSucceeded && !stoppingToken.IsCancellationRequested)
                {
                    await ResumeSchedulerAfterFailureAsync(scheduler);
                }

                lock (_statusLock)
                {
                    _active = false;
                }
            }
        }

        private async Task WaitForRunningJobsAsync(
            IScheduler scheduler,
            CancellationToken cancellationToken)
        {
            var lastReportedCount = -1;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var runningJobs = await scheduler.GetCurrentlyExecutingJobs();
                var runningCount = runningJobs.Count;

                if (runningCount == 0)
                {
                    UpdateStatus(status =>
                    {
                        status.Message = "同步任务已全部停止，准备迁移数据库";
                    }, forcePersist: true);
                    return;
                }

                if (runningCount != lastReportedCount)
                {
                    UpdateStatus(status =>
                    {
                        status.Message = $"已暂停新任务，正在等待 {runningCount} 个运行中的同步任务完成";
                    }, forcePersist: true);
                    lastReportedCount = runningCount;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private static async Task ResumeSchedulerAfterFailureAsync(IScheduler scheduler)
        {
            if (scheduler == null) return;

            try
            {
                if (!scheduler.IsShutdown)
                {
                    await scheduler.Start();
                    Log.Information("数据库迁移未完成，Quartz 同步任务调度已恢复");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "数据库迁移失败后恢复 Quartz 调度失败");
            }
        }

        private void UpdateProgress(DatabaseMigrationProgress progress)
        {
            UpdateStatus(status =>
            {
                status.SourceDbType = progress.SourceDbType;
                status.TargetDbType = progress.TargetDbType;
                status.CurrentTable = progress.CurrentTable;
                status.TableCount = progress.TableCount;
                status.CompletedTables = progress.CompletedTables;
                status.TotalRows = progress.TotalRows;
                status.MigratedRows = progress.MigratedRows;
                status.ProgressPercent = progress.TotalRows > 0
                    ? Math.Min(100, Math.Round(progress.MigratedRows * 100m / progress.TotalRows, 2))
                    : progress.TableCount > 0
                        ? Math.Min(100, Math.Round(progress.CompletedTables * 100m / progress.TableCount, 2))
                        : 0;
                status.Message = progress.Message;
            });
        }

        private void MarkFailed(string message)
        {
            UpdateStatus(status =>
            {
                status.State = DatabaseMigrationJobStates.Failed;
                status.Message = $"迁移失败：{message}";
                status.CompletedAt = DateTime.UtcNow;
                status.Restarting = false;
            }, forcePersist: true);
            AddHistory();
        }

        private void UpdateStatus(Action<DatabaseMigrationJobStatusDto> update, bool forcePersist = false)
        {
            lock (_statusLock)
            {
                update(_status);
                _status.UpdatedAt = DateTime.UtcNow;
                PersistStatus(forcePersist);
            }
        }

        private DatabaseMigrationJobStatusDto LoadStatus()
        {
            try
            {
                if (File.Exists(_configurationService.MigrationStatusFilePath))
                {
                    var loaded = JsonSerializer.Deserialize<DatabaseMigrationJobStatusDto>(
                        File.ReadAllText(_configurationService.MigrationStatusFilePath), JsonOptions());
                    if (loaded != null)
                    {
                        loaded.RefreshIntervalSeconds = RefreshIntervalSeconds;
                        return loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "读取数据库迁移状态失败，将使用空状态");
            }

            return new DatabaseMigrationJobStatusDto
            {
                RefreshIntervalSeconds = RefreshIntervalSeconds
            };
        }

        private List<DatabaseMigrationHistoryItemDto> LoadHistory()
        {
            try
            {
                if (File.Exists(_configurationService.MigrationHistoryFilePath))
                {
                    return JsonSerializer.Deserialize<List<DatabaseMigrationHistoryItemDto>>(
                        File.ReadAllText(_configurationService.MigrationHistoryFilePath), JsonOptions())
                        ?? new List<DatabaseMigrationHistoryItemDto>();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "读取数据库迁移历史失败，将使用空历史");
            }

            return new List<DatabaseMigrationHistoryItemDto>();
        }

        private void AddHistory()
        {
            lock (_statusLock)
            {
                if (string.IsNullOrWhiteSpace(_status.JobId)) return;

                _history.RemoveAll(item => item.JobId == _status.JobId);
                _history.Insert(0, new DatabaseMigrationHistoryItemDto
                {
                    JobId = _status.JobId,
                    State = _status.State,
                    SourceDbType = _status.SourceDbType,
                    TargetDbType = _status.TargetDbType,
                    TableCount = _status.TableCount,
                    TotalRows = _status.TotalRows,
                    MigratedRows = _status.MigratedRows,
                    Message = _status.Message,
                    StartedAt = _status.StartedAt,
                    CompletedAt = _status.CompletedAt
                });

                if (_history.Count > 50)
                {
                    _history.RemoveRange(50, _history.Count - 50);
                }

                PersistHistory();
            }
        }

        private void PersistHistory()
        {
            try
            {
                var path = _configurationService.MigrationHistoryFilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var tempPath = path + ".tmp";
                File.WriteAllText(tempPath, JsonSerializer.Serialize(_history, JsonOptions()));
                File.Move(tempPath, path, true);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "保存数据库迁移历史失败");
            }
        }

        private void PersistStatus(bool force)
        {
            var now = DateTime.UtcNow;
            if (!force && now - _lastPersistedAt < TimeSpan.FromSeconds(1)) return;

            try
            {
                var path = _configurationService.MigrationStatusFilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var tempPath = path + ".tmp";
                File.WriteAllText(tempPath, JsonSerializer.Serialize(_status, JsonOptions()));
                File.Move(tempPath, path, true);
                _lastPersistedAt = now;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "保存数据库迁移状态失败");
            }
        }

        private static DatabaseMigrationRequest CloneRequest(DatabaseMigrationRequest request) => new()
        {
            DbType = request.DbType,
            ConnectionString = request.ConnectionString,
            Host = request.Host,
            Port = request.Port,
            UserName = request.UserName,
            Password = request.Password,
            DatabaseName = request.DatabaseName
        };

        private static DatabaseMigrationJobStatusDto CloneStatus(DatabaseMigrationJobStatusDto status) => new()
        {
            JobId = status.JobId,
            State = status.State,
            SourceDbType = status.SourceDbType,
            TargetDbType = status.TargetDbType,
            CurrentTable = status.CurrentTable,
            TableCount = status.TableCount,
            CompletedTables = status.CompletedTables,
            TotalRows = status.TotalRows,
            MigratedRows = status.MigratedRows,
            ProgressPercent = status.ProgressPercent,
            Message = status.Message,
            StartedAt = status.StartedAt,
            UpdatedAt = status.UpdatedAt,
            CompletedAt = status.CompletedAt,
            Restarting = status.Restarting,
            RefreshIntervalSeconds = RefreshIntervalSeconds
        };

        private static DatabaseMigrationHistoryItemDto CloneHistoryItem(
            DatabaseMigrationHistoryItemDto item) => new()
        {
            JobId = item.JobId,
            State = item.State,
            SourceDbType = item.SourceDbType,
            TargetDbType = item.TargetDbType,
            TableCount = item.TableCount,
            TotalRows = item.TotalRows,
            MigratedRows = item.MigratedRows,
            Message = item.Message,
            StartedAt = item.StartedAt,
            CompletedAt = item.CompletedAt
        };

        private static JsonSerializerOptions JsonOptions() => new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
    }
}
