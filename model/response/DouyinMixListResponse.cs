using Newtonsoft.Json;

namespace dy.net.model.response
{
    /// <summary>
    /// 抖音合集列表响应根实体
    /// </summary>
    public class DouyinMixListResponse
    {
        /// <summary>
        /// 分页游标
        /// </summary>
        [JsonProperty("cursor")]
        public long Cursor { get; set; }

        ///// <summary>
        ///// 额外信息
        ///// </summary>
        //[JsonProperty("extra")]
        //public DouyinExtra Extra { get; set; }

        /// <summary>
        /// 是否有更多数据（0=无，1=有）
        /// </summary>
        [JsonProperty("has_more")]
        public int HasMore { get; set; }

        ///// <summary>
        ///// 日志信息
        ///// </summary>
        //[JsonProperty("log_pb")]
        //public DouyinLogPb LogPb { get; set; }

        /// <summary>
        /// 合集信息列表
        /// </summary>
        [JsonProperty("mix_infos")]
        public List<DouyinMixInfo> MixInfos { get; set; }

        /// <summary>
        /// 响应状态码（0=成功）
        /// </summary>
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
    }

    ///// <summary>
    ///// 额外信息实体
    ///// </summary>
    //public class DouyinExtra
    //{
    //    /// <summary>
    //    /// 致命项ID列表
    //    /// </summary>
    //    [JsonProperty("fatal_item_ids")]
    //    public List<object> FatalItemIds { get; set; }

    //    ///// <summary>
    //    ///// 日志ID
    //    ///// </summary>
    //    //[JsonProperty("logid")]
    //    //public string Logid { get; set; }

    //    /// <summary>
    //    /// 当前时间戳（毫秒）
    //    /// </summary>
    //    [JsonProperty("now")]
    //    public long Now { get; set; }
    //}

    ///// <summary>
    ///// 日志PB实体
    ///// </summary>
    //public class DouyinLogPb
    //{
    //    /// <summary>
    //    /// 曝光ID
    //    /// </summary>
    //    [JsonProperty("impr_id")]
    //    public string ImprId { get; set; }
    //}

    /// <summary>
    /// 合集信息实体
    /// </summary>
    public class DouyinMixInfo
    {
        /// <summary>
        /// 作者信息
        /// </summary>
        [JsonProperty("author")]
        public DouyinAuthor Author { get; set; }

        /// <summary>
        /// 合集封面
        /// </summary>
        [JsonProperty("cover_url")]
        public DouyinImageInfo CoverUrl { get; set; }

        /// <summary>
        /// 创建时间戳（秒）
        /// </summary>
        [JsonProperty("create_time")]
        public long CreateTime { get; set; }

        /// <summary>
        /// 合集描述
        /// </summary>
        [JsonProperty("desc")]
        public string Desc { get; set; }

        ///// <summary>
        ///// 合集额外信息（JSON字符串）
        ///// </summary>
        //[JsonProperty("extra")]
        //public string Extra { get; set; }

        ///// <summary>
        ///// ID列表（预留）
        ///// </summary>
        //[JsonProperty("ids")]
        //public object Ids { get; set; }

        ///// <summary>
        ///// 是否为IAA内容
        ///// </summary>
        //[JsonProperty("is_iaa")]
        //public int IsIaa { get; set; }

        ///// <summary>
        ///// 是否为系列合集
        ///// </summary>
        //[JsonProperty("is_serial_mix")]
        //public int IsSerialMix { get; set; }

        /// <summary>
        /// 合集ID
        /// </summary>
        [JsonProperty("mix_id")]
        public string MixId { get; set; }

        /// <summary>
        /// 合集名称
        /// </summary>
        [JsonProperty("mix_name")]
        public string MixName { get; set; }

        ///// <summary>
        ///// 合集图片类型
        ///// </summary>
        //[JsonProperty("mix_pic_type")]
        //public int MixPicType { get; set; }

        ///// <summary>
        ///// 合集类型
        ///// </summary>
        //[JsonProperty("mix_type")]
        //public int MixType { get; set; }

        ///// <summary>
        ///// 付费剧集（预留）
        ///// </summary>
        //[JsonProperty("paid_episodes")]
        //public object PaidEpisodes { get; set; }

        ///// <summary>
        ///// 分享信息
        ///// </summary>
        //[JsonProperty("share_info")]
        //public DouyinShareInfo ShareInfo { get; set; }

        ///// <summary>
        ///// 统计信息
        ///// </summary>
        //[JsonProperty("statis")]
        //public DouyinMixStatis Statis { get; set; }

        ///// <summary>
        ///// 状态信息
        ///// </summary>
        //[JsonProperty("status")]
        //public DouyinMixStatus Status { get; set; }

        ///// <summary>
        ///// 更新时间戳（秒）
        ///// </summary>
        //[JsonProperty("update_time")]
        //public long UpdateTime { get; set; }

        ///// <summary>
        ///// 已观看剧集数
        ///// </summary>
        //[JsonProperty("watched_episode")]
        //public int WatchedEpisode { get; set; }

        ///// <summary>
        ///// 已观看项ID
        ///// </summary>
        //[JsonProperty("watched_item")]
        //public string WatchedItem { get; set; }
    }

    /// <summary>
    /// 作者信息实体
    /// </summary>
    public class DouyinAuthor
    {
        ///// <summary>
        ///// 是否接受隐私政策
        ///// </summary>
        //[JsonProperty("accept_private_policy")]
        //public bool AcceptPrivatePolicy { get; set; }

        ///// <summary>
        ///// 账号地区
        ///// </summary>
        //[JsonProperty("account_region")]
        //public string AccountRegion { get; set; }

        ///// <summary>
        ///// 广告封面URL（预留）
        ///// </summary>
        //[JsonProperty("ad_cover_url")]
        //public object AdCoverUrl { get; set; }

        ///// <summary>
        ///// Apple账号（预留）
        ///// </summary>
        //[JsonProperty("apple_account")]
        //public int AppleAccount { get; set; }

        ///// <summary>
        ///// 权限状态
        ///// </summary>
        //[JsonProperty("authority_status")]
        //public int AuthorityStatus { get; set; }

        ///// <summary>
        ///// 168x168头像
        ///// </summary>
        //[JsonProperty("avatar_168x168")]
        //public DouyinImageInfo Avatar168x168 { get; set; }

        ///// <summary>
        ///// 300x300头像
        ///// </summary>
        //[JsonProperty("avatar_300x300")]
        //public DouyinImageInfo Avatar300x300 { get; set; }

        ///// <summary>
        ///// 大尺寸头像
        ///// </summary>
        //[JsonProperty("avatar_larger")]
        //public DouyinImageInfo AvatarLarger { get; set; }

        /// <summary>
        /// 中等尺寸头像
        /// </summary>
        [JsonProperty("avatar_medium")]
        public DouyinImageInfo AvatarMedium { get; set; }

        ///// <summary>
        ///// 头像方案列表（预留）
        ///// </summary>
        //[JsonProperty("avatar_schema_list")]
        //public object AvatarSchemaList { get; set; }

        ///// <summary>
        ///// 缩略头像
        ///// </summary>
        //[JsonProperty("avatar_thumb")]
        //public DouyinImageInfo AvatarThumb { get; set; }

        ///// <summary>
        ///// 头像URI
        ///// </summary>
        //[JsonProperty("avatar_uri")]
        //public string AvatarUri { get; set; }

        /// <summary>
        /// 作品控制权限
        /// </summary>
        [JsonProperty("aweme_control")]
        public DouyinAwemeControl AwemeControl { get; set; }

        /// <summary>
        /// 作品数量
        /// </summary>
        [JsonProperty("aweme_count")]
        public int AwemeCount { get; set; }

        /// <summary>
        /// 火山小视频认证
        /// </summary>
        [JsonProperty("aweme_hotsoon_auth")]
        public int AwemeHotsoonAuth { get; set; }

        /// <summary>
        /// 火山小视频认证关系
        /// </summary>
        [JsonProperty("aweme_hotsoon_auth_relation")]
        public int AwemeHotsoonAuthRelation { get; set; }

        /// <summary>
        /// 火山小视频问候信息
        /// </summary>
        [JsonProperty("awemehts_greet_info")]
        public string AwemehtsGreetInfo { get; set; }

        /// <summary>
        /// 禁用用户功能列表
        /// </summary>
        [JsonProperty("ban_user_functions")]
        public List<object> BanUserFunctions { get; set; }

        /// <summary>
        /// 星座
        /// </summary>
        [JsonProperty("constellation")]
        public int Constellation { get; set; }

        /// <summary>
        /// 联系人状态
        /// </summary>
        [JsonProperty("contacts_status")]
        public int ContactsStatus { get; set; }

        /// <summary>
        /// 封面URL列表
        /// </summary>
        [JsonProperty("cover_url")]
        public List<DouyinImageInfo> CoverUrl { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("create_time")]
        public int CreateTime { get; set; }

        /// <summary>
        /// 自定义认证
        /// </summary>
        [JsonProperty("custom_verify")]
        public string CustomVerify { get; set; }

        /// <summary>
        /// CV等级
        /// </summary>
        [JsonProperty("cv_level")]
        public string CvLevel { get; set; }

        /// <summary>
        /// 下载设置
        /// </summary>
        [JsonProperty("download_setting")]
        public int DownloadSetting { get; set; }

        /// <summary>
        /// 合拍设置
        /// </summary>
        [JsonProperty("duet_setting")]
        public int DuetSetting { get; set; }

        /// <summary>
        /// 企业认证理由
        /// </summary>
        [JsonProperty("enterprise_verify_reason")]
        public string EnterpriseVerifyReason { get; set; }

        /// <summary>
        /// 关注状态
        /// </summary>
        [JsonProperty("follow_status")]
        public int FollowStatus { get; set; }

        /// <summary>
        /// 粉丝数量
        /// </summary>
        [JsonProperty("follower_count")]
        public long FollowerCount { get; set; }

        /// <summary>
        /// 关注数量
        /// </summary>
        [JsonProperty("following_count")]
        public long FollowingCount { get; set; }

        /// <summary>
        /// 是否隐藏位置
        /// </summary>
        [JsonProperty("hide_location")]
        public bool HideLocation { get; set; }

        /// <summary>
        /// 是否隐藏搜索
        /// </summary>
        [JsonProperty("hide_search")]
        public bool HideSearch { get; set; }

        /// <summary>
        /// 是否为合集用户
        /// </summary>
        [JsonProperty("is_mix_user")]
        public bool IsMixUser { get; set; }

        /// <summary>
        /// 是否已认证
        /// </summary>
        [JsonProperty("is_verified")]
        public bool IsVerified { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        /// <summary>
        /// 安全UID
        /// </summary>
        [JsonProperty("sec_uid")]
        public string SecUid { get; set; }

        /// <summary>
        /// 短ID
        /// </summary>
        [JsonProperty("short_id")]
        public string ShortId { get; set; }

        /// <summary>
        /// 个性签名
        /// </summary>
        [JsonProperty("signature")]
        public string Signature { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// UID
        /// </summary>
        [JsonProperty("uid")]
        public string Uid { get; set; }

        /// <summary>
        /// 唯一ID
        /// </summary>
        [JsonProperty("unique_id")]
        public string UniqueId { get; set; }

        /// <summary>
        /// 认证类型
        /// </summary>
        [JsonProperty("verification_type")]
        public int VerificationType { get; set; }
    }

    /// <summary>
    /// 作品控制权限实体
    /// </summary>
    public class DouyinAwemeControl
    {
        /// <summary>
        /// 是否可评论
        /// </summary>
        [JsonProperty("can_comment")]
        public bool CanComment { get; set; }

        /// <summary>
        /// 是否可转发
        /// </summary>
        [JsonProperty("can_forward")]
        public bool CanForward { get; set; }

        /// <summary>
        /// 是否可分享
        /// </summary>
        [JsonProperty("can_share")]
        public bool CanShare { get; set; }

        /// <summary>
        /// 是否可显示评论
        /// </summary>
        [JsonProperty("can_show_comment")]
        public bool CanShowComment { get; set; }
    }

    /// <summary>
    /// 图片信息实体（头像/封面通用）
    /// </summary>
    public class DouyinImageInfo
    {
        /// <summary>
        /// 图片高度
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }

        /// <summary>
        /// 图片URI
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; set; }

        /// <summary>
        /// 图片URL列表（多CDN节点）
        /// </summary>
        [JsonProperty("url_list")]
        public List<string> UrlList { get; set; }

        /// <summary>
        /// 图片宽度
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }
    }

    /// <summary>
    /// 分享信息实体
    /// </summary>
    public class DouyinShareInfo
    {
        /// <summary>
        /// 分享描述
        /// </summary>
        [JsonProperty("share_desc")]
        public string ShareDesc { get; set; }

        /// <summary>
        /// 分享详细描述
        /// </summary>
        [JsonProperty("share_desc_info")]
        public string ShareDescInfo { get; set; }

        /// <summary>
        /// 分享标题
        /// </summary>
        [JsonProperty("share_title")]
        public string ShareTitle { get; set; }

        /// <summary>
        /// 自己查看的分享标题
        /// </summary>
        [JsonProperty("share_title_myself")]
        public string ShareTitleMyself { get; set; }

        /// <summary>
        /// 他人查看的分享标题
        /// </summary>
        [JsonProperty("share_title_other")]
        public string ShareTitleOther { get; set; }

        /// <summary>
        /// 分享URL
        /// </summary>
        [JsonProperty("share_url")]
        public string ShareUrl { get; set; }

        /// <summary>
        /// 微博分享描述
        /// </summary>
        [JsonProperty("share_weibo_desc")]
        public string ShareWeiboDesc { get; set; }
    }

    /// <summary>
    /// 合集统计信息实体
    /// </summary>
    public class DouyinMixStatis
    {
        /// <summary>
        /// 收藏播放量
        /// </summary>
        [JsonProperty("collect_vv")]
        public long CollectVv { get; set; }

        /// <summary>
        /// 当前剧集
        /// </summary>
        [JsonProperty("current_episode")]
        public int CurrentEpisode { get; set; }

        /// <summary>
        /// 是否有更新剧集
        /// </summary>
        [JsonProperty("has_updated_episode")]
        public int HasUpdatedEpisode { get; set; }

        /// <summary>
        /// 播放总量
        /// </summary>
        [JsonProperty("play_vv")]
        public long PlayVv { get; set; }

        /// <summary>
        /// 更新至剧集数
        /// </summary>
        [JsonProperty("updated_to_episode")]
        public int UpdatedToEpisode { get; set; }
    }

    /// <summary>
    /// 合集状态信息实体
    /// </summary>
    public class DouyinMixStatus
    {
        /// <summary>
        /// 是否已收藏（1=是，0=否）
        /// </summary>
        [JsonProperty("is_collected")]
        public int IsCollected { get; set; }

        /// <summary>
        /// 合集状态（2=正常）
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }
    }
}
