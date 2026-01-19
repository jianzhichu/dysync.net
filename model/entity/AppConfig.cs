using Newtonsoft.Json;
using SqlSugar;
using System.Text.RegularExpressions;

namespace dy.net.model.entity
{

    [SugarTable(TableName = "dy_app_config")]
    public class AppConfig
    {
        /// <summary>
        /// 是否是程序启动时...
        /// </summary>
        public bool IsFirstRunning { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true,Length =100)]
        public string Id { get; set; }

        [SugarColumn(Length =200,IsNullable =true)]
        public int Cron { get; set; }

        /// <summary>
        /// 每次查询数量
        /// </summary>
        public int BatchCount { get; set; } = 18;

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
        /// 图文视频 是否额外下载图片
        /// </summary>
        public bool DownImage { get; set; }
        /// <summary>
        /// 图文视频 是否额外下载mp3
        /// </summary>
        public bool DownMp3 { get; set; }

        /// <summary>
        /// 日志保留天数,防止容器日志太多，默认10天
        /// </summary>
        public int LogKeepDay { get; set; } = 10;
        /// <summary>
        /// 关注的视频标题命名模板{id}{VideoTitle}{SyncTime}{ReleaseTime}{FileHash}{Resolution}{FileSize}
        /// </summary>
        public string FollowedTitleTemplate { get; set; }
        /// <summary>
        /// 分隔符
        /// </summary>
        public string FollowedTitleSeparator { get; set; }
        /// <summary>
        /// 完整的标题模板，包含分隔符
        /// </summary>
        public string FullFollowedTitleTemplate { get; set; }


        /// <summary>
        /// 图文视频是否单独存放，否的话则按原类型存储位置存放，比如收藏夹、喜欢等
        /// </summary>
        public bool ImageViedoSaveAlone { get; set; }

     

        /// <summary>
        /// 自动去重-逻辑是遇到相同ID的视频直接跳过
        /// </summary>
        public bool AutoDistinct { get; set; }

        /// <summary>
        /// 去重优先级配置，json字符串格式存储
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string PriorityLevel { get; set; }

        /// <summary>
        /// 是否下载动态图视频
        /// </summary>
        public bool DownDynamicVideo { get; set; }

        /// <summary>
        /// 仅同步新视频（github 有人提议增加这个配置项，因为之前收藏了很多烂七八糟的视频。不想同步，又不想一个一个清除）
        /// </summary>
        public bool OnlySyncNew { get; set; } = true;
        /// <summary>
        /// 是否合并下载动态图视频
        /// </summary>
        public bool MegDynamicVideo { get; set; }
        /// <summary>
        /// 保留原动态视频文件
        /// </summary>
        public bool KeepDynamicVideo { get; set; }
    }

}
