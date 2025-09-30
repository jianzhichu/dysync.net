using SqlSugar;

namespace dy.net.model
{

    [SugarTable(TableName = "dy_collect_video")]
    public class DyCollectVideo
    {
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }

        [SugarColumn(Length =200,IsNullable =true)]
        public string DyUserId { get; set; }
        [SugarColumn(IsIgnore =true)]
        public string DyUser { get; set; }

        [SugarColumn(Length =200,IsNullable =true)]
        public string CookieId { get; set; }
        /// <summary>
        /// aweme_id
        /// </summary>
        [SugarColumn(Length =200,IsNullable =true)]
        public string AwemeId { get; set; }

        public DateTime SyncTime { get; set; }

        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 视频标题
        /// </summary>
        [SugarColumn(Length =2000,IsNullable =true)]
        public string VideoTitle { get; set; }
        /// <summary>
        /// 标签（分类）
        /// </summary>
        [SugarColumn(Length =200,IsNullable =true)]
        public string Tag1 { get; set; }

        /// <summary>
        /// 标签（分类）
        /// </summary>
        [SugarColumn(Length =200,IsNullable =true)]
        public string Tag2 { get; set; }

        [SugarColumn(Length =200,IsNullable =true)]
        public string Tag3 { get; set; }

        ///// <summary>
        ///// 视频描述
        ///// </summary>
        //public string VideoDesc { get; set; }
        /// <summary>
        /// 视频地址（解析会有多个取第一个）
        /// </summary>
        [SugarColumn(Length =0200,IsNullable =true)]
        public string VideoUrl { get; set; }
        /// <summary>
        /// 视频下载后保存路径
        /// </summary>
        [SugarColumn(Length =2000,IsNullable =true)]
        public string VideoSavePath { get; set; }
        /// <summary>
        /// 视频封面地址
        /// </summary>

        [SugarColumn(Length =2000,IsNullable =true)]
        public string VideoCoverUrl { get; set; }
        /// <summary>
        /// 视频封面保存路径
        /// </summary>
        [SugarColumn(Length =2000,IsNullable =true)]
        public string VideoCoverSavePath { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        [SugarColumn(Length =200,IsNullable =true)]
        public string Author { get; set; }
        /// <summary>
        /// 作者ID
        /// </summary>
        [SugarColumn(Length =200,IsNullable =true)]
        public string AuthorId { get; set; }
        /// <summary>
        /// 作者头像
        /// </summary>
        [SugarColumn(Length =2000,IsNullable =true)]
        public string AuthorAvatar { get; set; }

        [SugarColumn(Length =2000,IsNullable =true)]
        public string AuthorAvatarUrl { get; set; }

        /// <summary>
        /// 文件hash值唯一值
        /// </summary>
        [SugarColumn(Length =200,IsNullable =true)]
        public string FileHash { get; set; }

        /// <summary>
        /// 文件大小(字节)
        /// </summary>
        [SugarColumn(Length =200,IsNullable =true)]
        public long FileSize { get; set; }

        /// <summary>
        /// 分辨率（如1920x1080, 3840x2160等）
        /// </summary>
        [SugarColumn(Length =200,IsNullable =true)]
        public string Resolution { get; set; }

        /// <summary>
        /// 作者发布视频日期
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string createTimeStr => SyncTime.ToString("yyyy-MM-dd HH:mm:ss");
        [SugarColumn(IsIgnore = true)]
        public string SyncTimeStr => SyncTime.ToString("yyyy-MM-dd HH:mm:ss");

        [SugarColumn(Length =200,IsNullable =true)]
        public string ViedoType { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ViedoTypeStr  => ViedoType == "1" ? "我喜欢的" : "我收藏的";

        [SugarColumn(IsIgnore = true)]
        public string ViedoCate =>(string.IsNullOrEmpty(Tag1) ? "" : Tag1) + "/" + (string.IsNullOrEmpty(Tag2) ? "" : Tag2);
    }
}
