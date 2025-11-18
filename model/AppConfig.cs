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
        /// 博主视频是否 直接用标题做文件名，不另外加一层文件夹
        /// </summary>
        public bool UperUseViedoTitle { get; set; }
     
    }
}
