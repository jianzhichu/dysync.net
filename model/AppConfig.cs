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
    }
}
