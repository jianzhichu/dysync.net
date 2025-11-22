using SqlSugar;

namespace dy.net.model
{

    [SugarTable(TableName = "dy_app_config")]
    public class AppConfig
    {

        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true,Length =100)]
        public string Id { get; set; }

        [SugarColumn(Length =200,IsNullable =true)]
        public string Cron { get; set; }

        /// <summary>
        /// 每次查询数量
        /// </summary>
        public int BatchCount { get; set; } = 10;

        /// <summary>
        /// 博主视频是否 直接用标题做文件名
        /// </summary>
        public bool UperUseViedoTitle { get; set; }
        /// <summary>
        /// 是否博主视频直接放一个根目录，不另外按名字建文件夹
        /// </summary>
        public bool UperSaveTogether { get; set; }
        /// <summary>
        /// 是否下载图片视频
        /// </summary>
        public bool DownImageVideo { get; set; }
        /// <summary>
        /// 日志保留天数,防止容器日志太多，默认10天
        /// </summary>
        public int KeepLogDay { get; set; } = 10;

    }
}
