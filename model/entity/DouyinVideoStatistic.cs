using SqlSugar;

namespace dy.net.model.entity
{
    /// <summary>
    /// 视频全局统计。表中固定只有 Id=global 的一行。
    /// </summary>
    [SugarTable(TableName = "dy_video_statistic")]
    public class DouyinVideoStatistic
    {
        [SugarColumn(IsPrimaryKey = true, Length = 32)]
        public string Id { get; set; }

        public int SchemaVersion { get; set; }
        public long VideoCount { get; set; }
        public long AuthorCount { get; set; }
        public long CategoryCount { get; set; }
        public long TotalFileSize { get; set; }

        public long FavoriteCount { get; set; }
        public long FavoriteFileSize { get; set; }
        public long CollectCount { get; set; }
        public long CollectFileSize { get; set; }
        public long FollowCount { get; set; }
        public long FollowFileSize { get; set; }
        public long MixCount { get; set; }
        public long MixFileSize { get; set; }
        public long SeriesCount { get; set; }
        public long SeriesFileSize { get; set; }
        public long GraphicVideoCount { get; set; }
        public long GraphicVideoFileSize { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 按一级分类维护的视频数量。
    /// </summary>
    [SugarTable(TableName = "dy_video_category_statistic")]
    public class DouyinVideoCategoryStatistic
    {
        // 固定长度哈希键，避免 MySQL utf8mb4 复合字节导致主键索引超长。
        [SugarColumn(IsPrimaryKey = true, Length = 64)]
        public string CategoryKey { get; set; }

        [SugarColumn(Length = 200)]
        public string Name { get; set; }

        public long VideoCount { get; set; }
    }

    /// <summary>
    /// 按作者 ID 维护的视频数量，供作者统计分页和作者去重计数使用。
    /// </summary>
    [SugarTable(TableName = "dy_video_author_statistic")]
    public class DouyinVideoAuthorStatistic
    {
        // 固定长度哈希键，兼容 SQLite/MySQL/PostgreSQL 的主键索引限制。
        [SugarColumn(IsPrimaryKey = true, Length = 64)]
        public string AuthorKey { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string AuthorId { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Name { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", Length = -1, IsNullable = true)]
        public string Icon { get; set; }

        public long VideoCount { get; set; }
    }
}
