using Newtonsoft.Json;

namespace dy.net.model.response
{
    /// <summary>
    /// 收藏夹列表
    /// </summary>
    public class DouyinCollectListResponse
    {
        /// <summary>
        /// 收藏列表数据
        /// </summary>
        [JsonProperty("collects_list")]
        public List<DouyinCollectItem> CollectsList { get; set; } = new List<DouyinCollectItem>();

        /// <summary>
        /// 分页游标
        /// </summary>
        [JsonProperty("cursor")]
        public int Cursor { get; set; }

        /// <summary>
        /// 额外扩展信息
        /// </summary>
        //[JsonProperty("extra")]
        //public ExtraInfo Extra { get; set; } = new ExtraInfo();

        /// <summary>
        /// 是否还有更多数据
        /// </summary>
        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        /// <summary>
        /// 日志相关信息
        /// </summary>
        //[JsonProperty("log_pb")]
        //public LogPb LogPb { get; set; } = new LogPb();

        /// <summary>
        /// 响应状态码（0表示成功）
        /// </summary>
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }

        /// <summary>
        /// 收藏列表总数
        /// </summary>
        [JsonProperty("total_number")]
        public int TotalNumber { get; set; }
    }

    /// <summary>
    /// 单个收藏项实体
    /// </summary>
    public class DouyinCollectItem
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        [JsonProperty("app_id")]
        public int AppId { get; set; }

        /// <summary>
        /// 收藏封面信息
        /// </summary>
        //[JsonProperty("collects_cover")]
        //public CollectsCover CollectsCover { get; set; } = new CollectsCover();

        /// <summary>
        /// 收藏ID（数值型）
        /// </summary>
        [JsonProperty("collects_id")]
        public string CollectsId { get; set; }

        /// <summary>
        /// 收藏ID（字符串型）
        /// </summary>
        [JsonProperty("collects_id_str")]
        public string CollectsIdStr { get; set; } = string.Empty;

        /// <summary>
        /// 收藏名称
        /// </summary>
        [JsonProperty("collects_name")]
        public string CollectsName { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间（时间戳）
        /// </summary>
        [JsonProperty("create_time")]
        public long CreateTime { get; set; }

        /// <summary>
        /// 关注状态
        /// </summary>
        [JsonProperty("follow_status")]
        public int FollowStatus { get; set; }

        /// <summary>
        /// 被关注数量
        /// </summary>
        [JsonProperty("followed_count")]
        public int FollowedCount { get; set; }

        /// <summary>
        /// 是否为正常状态
        /// </summary>
        [JsonProperty("is_normal_status")]
        public bool IsNormalStatus { get; set; }

        /// <summary>
        /// 项目类型
        /// </summary>
        [JsonProperty("item_type")]
        public int ItemType { get; set; }

        /// <summary>
        /// 最后收藏时间（时间戳）
        /// </summary>
        [JsonProperty("last_collect_time")]
        public long LastCollectTime { get; set; }

        /// <summary>
        /// 播放数量
        /// </summary>
        [JsonProperty("play_count")]
        public int PlayCount { get; set; }

        /// <summary>
        /// 西西里收藏封面列表（暂为null，用可空对象接收）
        /// </summary>
        [JsonProperty("sicily_collects_cover_list")]
        public object? SicilyCollectsCoverList { get; set; }

        /// <summary>
        /// 状态标识
        /// </summary>
        [JsonProperty("states")]
        public int States { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// 系统类型
        /// </summary>
        [JsonProperty("system_type")]
        public int SystemType { get; set; }

        /// <summary>
        /// 该收藏下的项目总数
        /// </summary>
        [JsonProperty("total_number")]
        public int TotalNumber { get; set; }

        /// <summary>
        /// 用户ID（数值型）
        /// </summary>
        [JsonProperty("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// 用户ID（字符串型）
        /// </summary>
        [JsonProperty("user_id_str")]
        public string UserIdStr { get; set; } = string.Empty;

        /// <summary>
        /// 用户信息
        /// </summary>
        //[JsonProperty("user_info")]
        //public UserInfo UserInfo { get; set; } = new UserInfo();
    }

    /// <summary>
    /// 收藏封面信息实体
    /// </summary>
    //public class CollectsCover
    //{
    //    /// <summary>
    //    /// 封面资源URI
    //    /// </summary>
    //    [JsonProperty("uri")]
    //    public string Uri { get; set; } = string.Empty;

    //    /// <summary>
    //    /// 封面URL列表
    //    /// </summary>
    //    [JsonProperty("url_list")]
    //    public List<string> UrlList { get; set; } = new List<string>();
    //}

    /// <summary>
    /// 用户信息实体
    /// </summary>
    //public class UserInfo
    //{
    //    /// <summary>
    //    /// 大尺寸头像信息
    //    /// </summary>
    //    [JsonProperty("avatar_larger")]
    //    public AvatarInfo AvatarLarger { get; set; } = new AvatarInfo();

    //    /// <summary>
    //    /// 中等尺寸头像信息
    //    /// </summary>
    //    [JsonProperty("avatar_medium")]
    //    public AvatarInfo AvatarMedium { get; set; } = new AvatarInfo();

    //    /// <summary>
    //    /// 缩略图尺寸头像信息
    //    /// </summary>
    //    [JsonProperty("avatar_thumb")]
    //    public AvatarInfo AvatarThumb { get; set; } = new AvatarInfo();

    //    /// <summary>
    //    /// 用户昵称
    //    /// </summary>
    //    [JsonProperty("nickname")]
    //    public string Nickname { get; set; } = string.Empty;

    //    /// <summary>
    //    /// 用户唯一标识
    //    /// </summary>
    //    [JsonProperty("uid")]
    //    public string Uid { get; set; } = string.Empty;
    //}

    /// <summary>
    /// 头像信息实体
    /// </summary>
    //public class AvatarInfo
    //{
    //    /// <summary>
    //    /// 头像高度
    //    /// </summary>
    //    [JsonProperty("height")]
    //    public int Height { get; set; }

    //    /// <summary>
    //    /// 头像资源URI
    //    /// </summary>
    //    [JsonProperty("uri")]
    //    public string Uri { get; set; } = string.Empty;

    //    /// <summary>
    //    /// 头像URL列表
    //    /// </summary>
    //    [JsonProperty("url_list")]
    //    public List<string> UrlList { get; set; } = new List<string>();

    //    /// <summary>
    //    /// 头像宽度
    //    /// </summary>
    //    [JsonProperty("width")]
    //    public int Width { get; set; }
    //}

    /// <summary>
    /// 额外扩展信息实体
    /// </summary>
    //public class ExtraInfo
    //{
    //    /// <summary>
    //    /// 异常项目ID列表
    //    /// </summary>
    //    [JsonProperty("fatal_item_ids")]
    //    public List<long> FatalItemIds { get; set; } = new List<long>();

    //    /// <summary>
    //    /// 日志ID
    //    /// </summary>
    //    [JsonProperty("logid")]
    //    public string Logid { get; set; } = string.Empty;

    //    /// <summary>
    //    /// 当前时间戳（带毫秒）
    //    /// </summary>
    //    [JsonProperty("now")]
    //    public long Now { get; set; }
    //}

    /// <summary>
    /// 日志PB实体
    /// </summary>
    public class LogPb
    {
        /// <summary>
        /// 曝光ID
        /// </summary>
        [JsonProperty("impr_id")]
        public string ImprId { get; set; } = string.Empty;
    }
}
