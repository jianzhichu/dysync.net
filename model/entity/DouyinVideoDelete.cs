using SqlSugar;

namespace dy.net.model.entity
{
    /// <summary>
    /// 永久删除的视频记录-不再下载
    /// </summary>
    [SugarTable(TableName = "dy_delete_video")]
    public class DouyinVideoDelete
    {
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }
        /// <summary>
        /// 原视频Id
        /// </summary>
        [SugarColumn(IsNullable =true,Length =50)]
        public string ViedoId { get; set; }

        public DateTime DeleteTime { get; set; }

        [SugarColumn(IsNullable =true,Length =1000)]
        public string VideoTitle { get; set; }
        [SugarColumn(IsNullable = true, Length = 1000)]
        public string VideoSavePath { get; set; }
    }
}
