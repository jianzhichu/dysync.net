using SqlSugar;

namespace dy.net.model
{
    /// <summary>
    /// 重新下载的列表
    /// </summary>
    [SugarTable(TableName = "dy_rd_video")]
    public class ViedoReDown
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
        /// <summary>
        /// 原保存目录
        /// </summary>
        [SugarColumn(IsNullable =true,Length =1000)]
        public string SavePath { get; set; }

        public string CookieId { get; set; }
        /// <summary>
        /// 0-未下载 1-下载成功 2-下载失败
        /// </summary>
        public int Status { get; set; }

        public DateTime CreateTime { get; set; }
        public DateTime DownTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
