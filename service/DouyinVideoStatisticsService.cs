using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.utils;
using Serilog;
using SqlSugar;
using System.Security.Cryptography;
using System.Text;

namespace dy.net.service
{
    /// <summary>
    /// 持久化视频统计。所有差量更新都必须与视频数据变更处于同一数据库事务中。
    /// </summary>
    public class DouyinVideoStatisticsService
    {
        public const string GlobalStatisticId = "global";
        private const int CurrentSchemaVersion = 2;
        private const string OtherCategory = "其他";
        private readonly ISqlSugarClient _db;

        public DouyinVideoStatisticsService(ISqlSugarClient db)
        {
            _db = db;
        }

        public async Task EnsureInitializedAsync()
        {
            var statistic = await _db.Queryable<DouyinVideoStatistic>()
                    .Where(x => x.Id == GlobalStatisticId)
                    .FirstAsync();
            if (statistic == null || statistic.SchemaVersion != CurrentSchemaVersion)
            {
                Log.Information("视频增量统计需要初始化或升级，当前版本 {CurrentVersion}，目标版本 {TargetVersion}，开始从视频表重建",
                    statistic?.SchemaVersion ?? 0, CurrentSchemaVersion);
                await RebuildAsync();
            }
        }

        /// <summary>
        /// 仅在首次升级或人工修复时执行。聚合由数据库完成，不加载完整视频记录。
        /// </summary>
        public async Task RebuildAsync()
        {
            var typeRows = await _db.Queryable<DouyinVideo>()
                .GroupBy(x => x.ViedoType)
                .Select(x => new VideoTypeAggregate
                {
                    ViedoType = x.ViedoType,
                    VideoCount = SqlFunc.AggregateCount(x.Id),
                    FileSize = SqlFunc.AggregateSum(x.FileSize)
                })
                .ToListAsync();

            var categoryRows = await _db.Queryable<DouyinVideo>()
                .GroupBy(x => x.Tag1)
                .Select(x => new CategoryAggregate
                {
                    Name = x.Tag1,
                    VideoCount = SqlFunc.AggregateCount(x.Id)
                })
                .ToListAsync();

            var authorRows = await _db.Queryable<DouyinVideo>()
                .Where(x => !string.IsNullOrEmpty(x.AuthorId) || !string.IsNullOrEmpty(x.Author))
                .GroupBy(x => new { x.AuthorId, x.Author })
                .Select(x => new AuthorAggregate
                {
                    AuthorId = x.AuthorId,
                    Name = x.Author,
                    Icon = SqlFunc.AggregateMax(x.AuthorAvatarUrl),
                    VideoCount = SqlFunc.AggregateCount(x.Id)
                })
                .ToListAsync();

            var graphicRows = await _db.Queryable<DouyinVideo>()
                .Where(x => x.IsMergeVideo == 1)
                .GroupBy(x => x.IsMergeVideo)
                .Select(x => new CountAndSizeAggregate
                {
                    VideoCount = SqlFunc.AggregateCount(x.Id),
                    FileSize = SqlFunc.AggregateSum(x.FileSize)
                })
                .ToListAsync();

            var global = new DouyinVideoStatistic
            {
                Id = GlobalStatisticId,
                SchemaVersion = CurrentSchemaVersion,
                VideoCount = typeRows.Sum(x => x.VideoCount),
                TotalFileSize = typeRows.Sum(x => x.FileSize),
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var row in typeRows)
                ApplyTypeDelta(global, row.ViedoType, row.VideoCount, row.FileSize);

            var categories = categoryRows
                .GroupBy(x => NormalizeCategory(x.Name))
                .Select(x => new DouyinVideoCategoryStatistic
                {
                    CategoryKey = GetStableKey("category", x.Key),
                    Name = x.Key,
                    VideoCount = x.Sum(y => y.VideoCount)
                })
                .Where(x => x.VideoCount > 0)
                .ToList();

            var authors = authorRows
                .GroupBy(x => GetAuthorKey(x.AuthorId, x.Name))
                .Select(x =>
                {
                    var latest = x.Last();
                    return new DouyinVideoAuthorStatistic
                    {
                        AuthorKey = x.Key,
                        AuthorId = x.Select(y => y.AuthorId).LastOrDefault(y => !string.IsNullOrWhiteSpace(y)),
                        Name = x.Select(y => y.Name).LastOrDefault(y => !string.IsNullOrWhiteSpace(y)),
                        Icon = x.Select(y => y.Icon).LastOrDefault(y => !string.IsNullOrWhiteSpace(y)),
                        VideoCount = x.Sum(y => y.VideoCount)
                    };
                })
                .Where(x => x.VideoCount > 0)
                .ToList();

            global.CategoryCount = categories.Count;
            global.AuthorCount = authors.Count;
            var graphic = graphicRows.FirstOrDefault();
            global.GraphicVideoCount = graphic?.VideoCount ?? 0;
            global.GraphicVideoFileSize = graphic?.FileSize ?? 0;

            var result = await _db.Ado.UseTranAsync(async () =>
            {
                await _db.Deleteable<DouyinVideoCategoryStatistic>().ExecuteCommandAsync();
                await _db.Deleteable<DouyinVideoAuthorStatistic>().ExecuteCommandAsync();
                await _db.Deleteable<DouyinVideoStatistic>().ExecuteCommandAsync();
                await _db.Insertable(global).ExecuteCommandAsync();
                if (categories.Count > 0)
                    await _db.Insertable(categories).ExecuteCommandAsync();
                if (authors.Count > 0)
                    await _db.Insertable(authors).ExecuteCommandAsync();
            });

            if (!result.IsSuccess)
                throw result.ErrorException ?? new InvalidOperationException("重建视频统计失败");

            Log.Information("视频增量统计重建完成：视频 {VideoCount}，作者 {AuthorCount}，分类 {CategoryCount}",
                global.VideoCount, global.AuthorCount, global.CategoryCount);
        }

        /// <summary>
        /// 根据变更前后的实体快照更新统计。调用方负责开启数据库事务。
        /// </summary>
        public async Task ApplyChangesAsync(
            IEnumerable<DouyinVideo> oldVideos,
            IEnumerable<DouyinVideo> newVideos)
        {
            var oldList = oldVideos?.ToList() ?? new List<DouyinVideo>();
            var newList = newVideos?.ToList() ?? new List<DouyinVideo>();
            if (oldList.Count == 0 && newList.Count == 0)
                return;

            var globalQuery = _db.Queryable<DouyinVideoStatistic>()
                .Where(x => x.Id == GlobalStatisticId);
            // MySQL/PostgreSQL 可被多个应用实例同时连接。锁定全局汇总行，
            // 使后续全局、分类、作者差量在数据库层串行，避免读改写丢失更新。
            // SQLite 由数据库自身的单写者事务保证，不能生成 FOR UPDATE。
            if (_db.CurrentConnectionConfig.DbType != DbType.Sqlite)
                globalQuery = globalQuery.TranLock(DbLockType.Wait);
            var global = await globalQuery.FirstAsync();
            if (global == null)
                throw new InvalidOperationException("视频增量统计未初始化，已取消视频数据变更");

            var categoryDeltas = new Dictionary<string, DimensionDelta>(StringComparer.Ordinal);
            var authorDeltas = new Dictionary<string, DimensionDelta>(StringComparer.Ordinal);

            foreach (var video in oldList)
                ApplyVideoDelta(global, categoryDeltas, authorDeltas, video, -1);
            foreach (var video in newList)
                ApplyVideoDelta(global, categoryDeltas, authorDeltas, video, 1);

            global.CategoryCount += await ApplyCategoryDeltasAsync(categoryDeltas);
            global.AuthorCount += await ApplyAuthorDeltasAsync(authorDeltas);
            global.UpdatedAt = DateTime.UtcNow;
            ValidateGlobal(global);
            await _db.Updateable(global).ExecuteCommandAsync();
        }

        public async Task<VideoStaticsDto> GetStaticsAsync()
        {
            await EnsureInitializedAsync();
            var global = await _db.Queryable<DouyinVideoStatistic>()
                .Where(x => x.Id == GlobalStatisticId)
                .FirstAsync() ?? new DouyinVideoStatistic();
            var categories = await _db.Queryable<DouyinVideoCategoryStatistic>()
                .OrderBy(x => x.VideoCount, OrderByType.Desc)
                .OrderBy(x => x.Name, OrderByType.Asc)
                .ToListAsync();

            return new VideoStaticsDto
            {
                VideoCount = global.VideoCount,
                AuthorCount = global.AuthorCount,
                CategoryCount = global.CategoryCount,
                FavoriteCount = global.FavoriteCount,
                CollectCount = global.CollectCount,
                FollowCount = global.FollowCount,
                MixCount = global.MixCount,
                SeriesCount = global.SeriesCount,
                GraphicVideoCount = global.GraphicVideoCount,
                VideoSizeTotal = FormatSize(global.TotalFileSize),
                VideoFavoriteSize = FormatSize(global.FavoriteFileSize),
                VideoCollectSize = FormatSize(global.CollectFileSize),
                VideoFollowSize = FormatSize(global.FollowFileSize),
                VideoMixSize = FormatSize(global.MixFileSize),
                VideoSeriesSize = FormatSize(global.SeriesFileSize),
                GraphicVideoSize = FormatSize(global.GraphicVideoFileSize),
                Categories = categories.Select(x => new VideoStaticsItemDto
                {
                    Name = x.Name,
                    Count = x.VideoCount
                }).ToList()
            };
        }

        public async Task<(List<VideoStaticsItemDto> list, int totalCount)> GetAuthorsPagedAsync(
            int pageIndex,
            int pageSize)
        {
            await EnsureInitializedAsync();
            var query = _db.Queryable<DouyinVideoAuthorStatistic>();
            var totalCount = await query.CountAsync();
            var rows = await _db.Queryable<DouyinVideoAuthorStatistic>()
                .OrderBy(x => x.VideoCount, OrderByType.Desc)
                .OrderBy(x => x.Name, OrderByType.Asc)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (rows.Select(x => new VideoStaticsItemDto
            {
                Name = x.Name,
                Count = x.VideoCount,
                Icon = x.Icon,
                UperId = x.AuthorId
            }).ToList(), totalCount);
        }

        private async Task<long> ApplyCategoryDeltasAsync(Dictionary<string, DimensionDelta> deltas)
        {
            if (deltas.Count == 0)
                return 0;

            var keys = deltas.Keys.ToList();
            var existing = await _db.Queryable<DouyinVideoCategoryStatistic>()
                .Where(x => keys.Contains(x.CategoryKey))
                .ToListAsync();
            var existingMap = existing.ToDictionary(x => x.CategoryKey, StringComparer.Ordinal);
            var inserts = new List<DouyinVideoCategoryStatistic>();
            var updates = new List<DouyinVideoCategoryStatistic>();
            var deletes = new List<string>();

            foreach (var pair in deltas)
            {
                if (!existingMap.TryGetValue(pair.Key, out var row))
                {
                    if (pair.Value.Count <= 0)
                        throw new InvalidOperationException($"分类统计缺失：{pair.Key}");
                    inserts.Add(new DouyinVideoCategoryStatistic
                    {
                        CategoryKey = pair.Key,
                        Name = pair.Value.Name,
                        VideoCount = pair.Value.Count
                    });
                    continue;
                }

                row.VideoCount += pair.Value.Count;
                if (row.VideoCount < 0)
                    throw new InvalidOperationException($"分类统计出现负数：{pair.Key}");
                if (row.VideoCount == 0)
                    deletes.Add(row.CategoryKey);
                else
                    updates.Add(row);
            }

            if (inserts.Count > 0) await _db.Insertable(inserts).ExecuteCommandAsync();
            if (updates.Count > 0) await _db.Updateable(updates).ExecuteCommandAsync();
            if (deletes.Count > 0)
                await _db.Deleteable<DouyinVideoCategoryStatistic>()
                    .Where(x => deletes.Contains(x.CategoryKey)).ExecuteCommandAsync();
            return inserts.Count - deletes.Count;
        }

        private async Task<long> ApplyAuthorDeltasAsync(Dictionary<string, DimensionDelta> deltas)
        {
            if (deltas.Count == 0)
                return 0;

            var keys = deltas.Keys.ToList();
            var existing = await _db.Queryable<DouyinVideoAuthorStatistic>()
                .Where(x => keys.Contains(x.AuthorKey))
                .ToListAsync();
            var existingMap = existing.ToDictionary(x => x.AuthorKey, StringComparer.Ordinal);
            var inserts = new List<DouyinVideoAuthorStatistic>();
            var updates = new List<DouyinVideoAuthorStatistic>();
            var deletes = new List<string>();

            foreach (var pair in deltas)
            {
                if (!existingMap.TryGetValue(pair.Key, out var row))
                {
                    if (pair.Value.Count <= 0)
                        throw new InvalidOperationException($"作者统计缺失：{pair.Key}");
                    inserts.Add(new DouyinVideoAuthorStatistic
                    {
                        AuthorKey = pair.Key,
                        AuthorId = pair.Value.UperId,
                        Name = pair.Value.Name,
                        Icon = pair.Value.Icon,
                        VideoCount = pair.Value.Count
                    });
                    continue;
                }

                row.VideoCount += pair.Value.Count;
                if (row.VideoCount < 0)
                    throw new InvalidOperationException($"作者统计出现负数：{pair.Key}");
                if (row.VideoCount == 0)
                {
                    deletes.Add(row.AuthorKey);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(pair.Value.UperId)) row.AuthorId = pair.Value.UperId;
                    if (!string.IsNullOrWhiteSpace(pair.Value.Name)) row.Name = pair.Value.Name;
                    if (!string.IsNullOrWhiteSpace(pair.Value.Icon)) row.Icon = pair.Value.Icon;
                    updates.Add(row);
                }
            }

            if (inserts.Count > 0) await _db.Insertable(inserts).ExecuteCommandAsync();
            if (updates.Count > 0) await _db.Updateable(updates).ExecuteCommandAsync();
            if (deletes.Count > 0)
                await _db.Deleteable<DouyinVideoAuthorStatistic>()
                    .Where(x => deletes.Contains(x.AuthorKey)).ExecuteCommandAsync();
            return inserts.Count - deletes.Count;
        }

        private static void ApplyVideoDelta(
            DouyinVideoStatistic global,
            Dictionary<string, DimensionDelta> categoryDeltas,
            Dictionary<string, DimensionDelta> authorDeltas,
            DouyinVideo video,
            long direction)
        {
            var sizeDelta = Math.Max(0, video.FileSize) * direction;
            global.VideoCount += direction;
            global.TotalFileSize += sizeDelta;
            ApplyTypeDelta(global, video.ViedoType, direction, sizeDelta);

            if (video.IsMergeVideo == 1)
            {
                global.GraphicVideoCount += direction;
                global.GraphicVideoFileSize += sizeDelta;
            }

            var categoryName = NormalizeCategory(video.Tag1);
            AddDimensionDelta(categoryDeltas, GetStableKey("category", categoryName), direction,
                categoryName, null, null);

            if (!string.IsNullOrWhiteSpace(video.AuthorId) || !string.IsNullOrWhiteSpace(video.Author))
            {
                AddDimensionDelta(authorDeltas, GetAuthorKey(video.AuthorId, video.Author), direction,
                    video.Author, video.AuthorAvatarUrl, video.AuthorId);
            }
        }

        private static void ApplyTypeDelta(
            DouyinVideoStatistic global,
            VideoTypeEnum type,
            long countDelta,
            long sizeDelta)
        {
            switch (type)
            {
                case VideoTypeEnum.dy_favorite:
                    global.FavoriteCount += countDelta;
                    global.FavoriteFileSize += sizeDelta;
                    break;
                case VideoTypeEnum.dy_collects:
                case VideoTypeEnum.dy_custom_collect:
                    global.CollectCount += countDelta;
                    global.CollectFileSize += sizeDelta;
                    break;
                case VideoTypeEnum.dy_follows:
                    global.FollowCount += countDelta;
                    global.FollowFileSize += sizeDelta;
                    break;
                case VideoTypeEnum.dy_mix:
                    global.MixCount += countDelta;
                    global.MixFileSize += sizeDelta;
                    break;
                case VideoTypeEnum.dy_series:
                    global.SeriesCount += countDelta;
                    global.SeriesFileSize += sizeDelta;
                    break;
            }
        }

        private static void AddDimensionDelta(
            Dictionary<string, DimensionDelta> deltas,
            string key,
            long count,
            string name,
            string icon,
            string uperId)
        {
            if (!deltas.TryGetValue(key, out var delta))
            {
                delta = new DimensionDelta();
                deltas[key] = delta;
            }
            delta.Count += count;
            if (count > 0)
            {
                delta.Name = name;
                delta.Icon = icon;
                delta.UperId = uperId;
            }
        }

        private static string NormalizeCategory(string category) =>
            string.IsNullOrWhiteSpace(category) ? OtherCategory : category;

        private static string GetAuthorKey(string authorId, string authorName) =>
            GetStableKey("author", !string.IsNullOrWhiteSpace(authorId) ? $"id:{authorId}" : $"name:{authorName}");

        private static string GetStableKey(string dimension, string value)
        {
            var bytes = Encoding.UTF8.GetBytes($"{dimension}:{value}");
            return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        }

        private static string FormatSize(long bytes)
        {
            var value = DouyinFileUtils.ConvertBytesToGb(Math.Max(0, bytes));
            return bytes > 0 && value == "0.00" ? "<0.01" : value;
        }

        private static void ValidateGlobal(DouyinVideoStatistic value)
        {
            var numbers = new[]
            {
                value.VideoCount, value.AuthorCount, value.CategoryCount, value.TotalFileSize,
                value.FavoriteCount, value.FavoriteFileSize, value.CollectCount, value.CollectFileSize,
                value.FollowCount, value.FollowFileSize, value.MixCount, value.MixFileSize,
                value.SeriesCount, value.SeriesFileSize, value.GraphicVideoCount,
                value.GraphicVideoFileSize
            };
            if (numbers.Any(x => x < 0))
                throw new InvalidOperationException("视频增量统计出现负数，已取消本次数据变更，请重建统计");
        }

        private sealed class DimensionDelta
        {
            public long Count { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public string UperId { get; set; }
        }

        private sealed class VideoTypeAggregate
        {
            public VideoTypeEnum ViedoType { get; set; }
            public long VideoCount { get; set; }
            public long FileSize { get; set; }
        }

        private sealed class CategoryAggregate
        {
            public string Name { get; set; }
            public long VideoCount { get; set; }
        }

        private sealed class AuthorAggregate
        {
            public string AuthorId { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public long VideoCount { get; set; }
        }

        private sealed class CountAndSizeAggregate
        {
            public long VideoCount { get; set; }
            public long FileSize { get; set; }
        }
    }
}
