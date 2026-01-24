using Newtonsoft.Json;
namespace dy.net.model.response
{

    /// <summary>
    /// 视频列表
    /// </summary>
    public class DouyinVideoInfoResponse
    {
        [JsonProperty("aweme_list")]
        public List<Aweme> AwemeList { get; set; }

        [JsonProperty("cursor")]
        public string Cursor { get; set; }

        [JsonProperty("max_cursor")]
        public string MaxCursor { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("has_more")]
        public int HasMore { get; set; }
    }

    public class Aweme
    {
        //[JsonProperty("activity_video_type")]
        //public int ActivityVideoType { get; set; }

        //[JsonProperty("authentication_token")]
        //public string AuthenticationToken { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        //[JsonProperty("author_mask_tag")]
        //public int AuthorMaskTag { get; set; }

        [JsonProperty("author_user_id")]
        public long AuthorUserId { get; set; }

        //[JsonProperty("aweme_control")]
        //public AwemeControl AwemeControl { get; set; }

        [JsonProperty("aweme_id")]
        public string AwemeId { get; set; }

        //[JsonProperty("aweme_listen_struct")]
        //public AwemeListenStruct AwemeListenStruct { get; set; }

        //[JsonProperty("aweme_type")]
        //public int AwemeType { get; set; }

        //[JsonProperty("aweme_type_tags")]
        //public string AwemeTypeTags { get; set; }

        //[JsonProperty("boost_status")]
        //public int BoostStatus { get; set; }

        //[JsonProperty("can_cache_to_local")]
        //public bool CanCacheToLocal { get; set; }

        //[JsonProperty("caption")]
        //public string Caption { get; set; }

        //[JsonProperty("caption_template_id")]
        //public int CaptionTemplateId { get; set; }

        //[JsonProperty("cf_assets_type")]
        //public int CfAssetsType { get; set; }

        //[JsonProperty("chapter_list")]
        //public object ChapterList { get; set; }

        //[JsonProperty("collect_stat")]
        //public int CollectStat { get; set; }

        //[JsonProperty("collection_corner_mark")]
        //public int CollectionCornerMark { get; set; }

        //[JsonProperty("comment_gid")]
        //public string CommentGid { get; set; }

        //[JsonProperty("component_control")]
        //public ComponentControl ComponentControl { get; set; }

        //[JsonProperty("component_info_v2")]
        //public string ComponentInfoV2 { get; set; }

        //[JsonProperty("cover_labels")]
        //public object CoverLabels { get; set; }

        [JsonProperty("create_time")]
        public long CreateTime { get; set; }
        /// <summary>
        /// 视频标题
        /// </summary>
        [JsonProperty("desc")]
        public string Desc { get; set; }

        //[JsonProperty("descendants")]
        //public Descendants Descendants { get; set; }

        //[JsonProperty("disable_relation_bar")]
        //public int DisableRelationBar { get; set; }

        //[JsonProperty("douplus_user_type")]
        //public int DouplusUserType { get; set; }

        //[JsonProperty("douyin_p_c_video_extra")]
        //public string DouyinPCVideoExtra { get; set; }

        //[JsonProperty("enable_comment_sticker_rec")]
        //public bool EnableCommentStickerRec { get; set; }

        //[JsonProperty("ent_log_extra")]
        //public EntLogExtra EntLogExtra { get; set; }

        //[JsonProperty("entertainment_product_info")]
        //public EntertainmentProductInfo EntertainmentProductInfo { get; set; }

        //[JsonProperty("entertainment_video_paid_way")]
        //public EntertainmentVideoPaidWay EntertainmentVideoPaidWay { get; set; }

        //[JsonProperty("entertainment_video_type")]
        //public int EntertainmentVideoType { get; set; }

        //[JsonProperty("feed_comment_config")]
        //public FeedCommentConfig FeedCommentConfig { get; set; }

        //[JsonProperty("flash_mob_trends")]
        //public int FlashMobTrends { get; set; }

        //[JsonProperty("follow_shoot_clip_info")]
        //public FollowShootClipInfo FollowShootClipInfo { get; set; }

        //[JsonProperty("friend_recommend_info")]
        //public FriendRecommendInfo FriendRecommendInfo { get; set; }

        //[JsonProperty("game_tag_info")]
        //public GameTagInfo GameTagInfo { get; set; }

        //[JsonProperty("group_id")]
        //public string GroupId { get; set; }

        //[JsonProperty("have_dashboard")]
        //public bool HaveDashboard { get; set; }

        //[JsonProperty("image_album_music_info")]
        //public ImageAlbumMusicInfo ImageAlbumMusicInfo { get; set; }

        //[JsonProperty("image_comment")]
        //public ImageComment ImageComment { get; set; }

        //[JsonProperty("image_crop_ctrl")]
        //public int ImageCropCtrl { get; set; }

        //[JsonProperty("image_list")]
        //public object ImageList { get; set; }

        [JsonProperty("images")]
        public List<ImageItemInfo> Images { get; set; }

        //[JsonProperty("img_bitrate")]
        //public object ImgBitrate { get; set; }

        //[JsonProperty("impression_data")]
        //public ImpressionData ImpressionData { get; set; }

        //[JsonProperty("is_24_story")]
        //public int Is24Story { get; set; }

        //[JsonProperty("is_25_story")]
        //public int Is25Story { get; set; }

        //[JsonProperty("is_ads")]
        //public bool IsAds { get; set; }

        //[JsonProperty("is_collects_selected")]
        //public int IsCollectsSelected { get; set; }

        //[JsonProperty("is_duet_sing")]
        //public bool IsDuetSing { get; set; }

        //[JsonProperty("is_first_video")]
        //public bool IsFirstVideo { get; set; }

        //[JsonProperty("is_from_ad_auth")]
        //public bool IsFromAdAuth { get; set; }

        //[JsonProperty("is_image_beat")]
        //public bool IsImageBeat { get; set; }

        //[JsonProperty("is_karaoke")]
        //public bool IsKaraoke { get; set; }

        //[JsonProperty("is_life_item")]
        //public bool IsLifeItem { get; set; }

        //[JsonProperty("is_moment_history")]
        //public int IsMomentHistory { get; set; }

        //[JsonProperty("is_moment_story")]
        //public int IsMomentStory { get; set; }

        //[JsonProperty("is_new_text_mode")]
        //public int IsNewTextMode { get; set; }

        //[JsonProperty("is_preview")]
        //public int IsPreview { get; set; }

        //[JsonProperty("is_share_post")]
        //public bool IsSharePost { get; set; }

        //[JsonProperty("is_story")]
        //public int IsStory { get; set; }

        //[JsonProperty("is_top")]
        //public int IsTop { get; set; }

        [JsonProperty("is_use_music")]
        public bool IsUseMusic { get; set; }

        //[JsonProperty("item_stitch")]
        //public int ItemStitch { get; set; }

        //[JsonProperty("item_title")]
        //public string ItemTitle { get; set; }

        //[JsonProperty("item_warn_notification")]
        //public ItemWarnNotification ItemWarnNotification { get; set; }

        //[JsonProperty("libfinsert_task_id")]
        //public string LibfinsertTaskId { get; set; }

        //[JsonProperty("life_video_collect_info")]
        //public string LifeVideoCollectInfo { get; set; }

        //[JsonProperty("mark_largely_following")]
        //public bool MarkLargelyFollowing { get; set; }

        //[JsonProperty("media_type")]
        //public int MediaType { get; set; }

        [JsonProperty("mix_info")]
        public MixInfo MixInfo { get; set; }

        [JsonProperty("music")]
        public Music Music { get; set; }

        //[JsonProperty("origin_duet_resource_uri")]
        //public string OriginDuetResourceUri { get; set; }

        //[JsonProperty("original")]
        //public int Original { get; set; }

        //[JsonProperty("original_images")]
        //public object OriginalImages { get; set; }

        //[JsonProperty("packed_clips")]
        //public object PackedClips { get; set; }

        //[JsonProperty("personal_page_botton_diagnose_style")]
        //public int PersonalPageBottonDiagnoseStyle { get; set; }

        //[JsonProperty("photo_search_entrance")]
        //public PhotoSearchEntrance PhotoSearchEntrance { get; set; }

        //[JsonProperty("poi_patch_info")]
        //public PoiPatchInfo PoiPatchInfo { get; set; }

        //[JsonProperty("prevent_download")]
        //public bool PreventDownload { get; set; }

        //[JsonProperty("publish_plus_alienation")]
        //public PublishPlusAlienation PublishPlusAlienation { get; set; }

        //[JsonProperty("region")]
        //public string Region { get; set; }

        //[JsonProperty("related_video_extra")]
        //public RelatedVideoExtra RelatedVideoExtra { get; set; }

        //[JsonProperty("risk_infos")]
        //public RiskInfos RiskInfos { get; set; }

        //[JsonProperty("series_basic_info")]
        //public SeriesBasicInfo SeriesBasicInfo { get; set; }

        //[JsonProperty("series_paid_info")]
        //public SeriesPaidInfo SeriesPaidInfo { get; set; }

        //[JsonProperty("share_info")]
        //public ShareInfo ShareInfo { get; set; }

        //[JsonProperty("share_url")]
        //public string ShareUrl { get; set; }

        //[JsonProperty("shoot_way")]
        //public string ShootWay { get; set; }

        //[JsonProperty("statistics")]
        //public Statistics Statistics { get; set; }

        //[JsonProperty("status")]
        //public Status Status { get; set; }

        //[JsonProperty("text_extra")]
        //public List<TextExtra> TextExtra { get; set; }

        //[JsonProperty("trends_event_track")]
        //public string TrendsEventTrack { get; set; }

        //[JsonProperty("user_digged")]
        //public int UserDigged { get; set; }

        //[JsonProperty("user_recommend_status")]
        //public int UserRecommendStatus { get; set; }

        [JsonProperty("video")]
        public Video Video { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("video_tag")]
        public List<VideoTagItem> VideoTags { get; set; }
    }


    public class VideoTagItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("level")]
        public int Level { get; set; }
        /// <summary>
        /// 改成long 防止报错
        /// </summary>
        [JsonProperty("tag_id")]
        public long TagId { get; set; }
        /// <summary>
        /// 人文社科
        /// </summary>
        [JsonProperty("tag_name")]
        public string TagName { get; set; }
    }

    public class Author
    {
        //[JsonProperty("account_cert_info")]
        //public string AccountCertInfo { get; set; }

        [JsonProperty("avatar_larger")]
        public ImageInfo AvatarLarger { get; set; }

        [JsonProperty("avatar_thumb")]
        public ImageInfo AvatarThumb { get; set; }

        //[JsonProperty("cover_url")]
        //public List<ImageInfo> CoverUrl { get; set; }

        //[JsonProperty("custom_verify")]
        //public string CustomVerify { get; set; }

        //[JsonProperty("enterprise_verify_reason")]
        //public string EnterpriseVerifyReason { get; set; }

        //[JsonProperty("follow_status")]
        //public int FollowStatus { get; set; }

        //[JsonProperty("follower_status")]
        //public int FollowerStatus { get; set; }

        //[JsonProperty("is_ad_fake")]
        //public bool IsAdFake { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        //[JsonProperty("prevent_download")]
        //public bool PreventDownload { get; set; }

        //[JsonProperty("risk_notice_text")]
        //public string RiskNoticeText { get; set; }

        //[JsonProperty("sec_uid")]
        //public string SecUid { get; set; }

        //[JsonProperty("share_info")]
        //public AuthorShareInfo ShareInfo { get; set; }

        //[JsonProperty("stitch_setting")]
        //public int StitchSetting { get; set; }

        //[JsonProperty("story_interactive")]
        //public int StoryInteractive { get; set; }

        //[JsonProperty("story_ring")]
        //public StoryRing StoryRing { get; set; }

        //[JsonProperty("story_ttl")]
        //public int StoryTtl { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }
    }

    public class ImageInfo
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        //[JsonProperty("uri")]
        //public string Uri { get; set; }

        [JsonProperty("url_list")]
        public List<string> UrlList { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }

    //public class AuthorShareInfo
    //{
    //    [JsonProperty("share_desc")]
    //    public string ShareDesc { get; set; }

    //    [JsonProperty("share_desc_info")]
    //    public string ShareDescInfo { get; set; }

    //    [JsonProperty("share_qrcode_url")]
    //    public ImageInfo ShareQrcodeUrl { get; set; }

    //    [JsonProperty("share_title")]
    //    public string ShareTitle { get; set; }

    //    [JsonProperty("share_title_myself")]
    //    public string ShareTitleMyself { get; set; }

    //    [JsonProperty("share_title_other")]
    //    public string ShareTitleOther { get; set; }

    //    [JsonProperty("share_url")]
    //    public string ShareUrl { get; set; }

    //    [JsonProperty("share_weibo_desc")]
    //    public string ShareWeiboDesc { get; set; }
    //}

    //public class StoryRing
    //{
    //    [JsonProperty("story_rings")]
    //    public List<object> StoryRings { get; set; }

    //    [JsonProperty("version")]
    //    public long Version { get; set; }
    //}

    //public class AwemeControl
    //{
    //    [JsonProperty("can_comment")]
    //    public bool CanComment { get; set; }

    //    [JsonProperty("can_forward")]
    //    public bool CanForward { get; set; }

    //    [JsonProperty("can_share")]
    //    public bool CanShare { get; set; }

    //    [JsonProperty("can_show_comment")]
    //    public bool CanShowComment { get; set; }
    //}

    //public class AwemeListenStruct
    //{
    //    [JsonProperty("trace_info")]
    //    public string TraceInfo { get; set; }
    //}

    //public class ComponentControl
    //{
    //    [JsonProperty("data_source_url")]
    //    public string DataSourceUrl { get; set; }
    //}

    //public class Descendants
    //{
    //    [JsonProperty("notify_msg")]
    //    public string NotifyMsg { get; set; }

    //    [JsonProperty("platforms")]
    //    public List<string> Platforms { get; set; }
    //}

    //public class EntLogExtra
    //{
    //    [JsonProperty("log_extra")]
    //    public string LogExtra { get; set; }
    //}

    //public class EntertainmentProductInfo
    //{
    //    [JsonProperty("market_info")]
    //    public MarketInfo MarketInfo { get; set; }
    //}

    //public class MarketInfo
    //{
    //    [JsonProperty("limit_free")]
    //    public LimitFree LimitFree { get; set; }
    //}

    //public class LimitFree
    //{
    //    [JsonProperty("in_free")]
    //    public bool InFree { get; set; }
    //}

    //public class EntertainmentVideoPaidWay
    //{
    //    [JsonProperty("enable_use_new_ent_data")]
    //    public bool EnableUseNewEntData { get; set; }

    //    [JsonProperty("paid_type")]
    //    public int PaidType { get; set; }

    //    [JsonProperty("paid_ways")]
    //    public List<object> PaidWays { get; set; }
    //}

    //public class FeedCommentConfig
    //{
    //    [JsonProperty("author_audit_status")]
    //    public int AuthorAuditStatus { get; set; }

    //    [JsonProperty("common_flags")]
    //    public string CommonFlags { get; set; }

    //    [JsonProperty("input_config_text")]
    //    public string InputConfigText { get; set; }
    //}

    //public class FollowShootClipInfo
    //{
    //    [JsonProperty("clip_from_user")]
    //    public long ClipFromUser { get; set; }

    //    [JsonProperty("clip_video_all")]
    //    public long ClipVideoAll { get; set; }
    //}

    //public class FriendRecommendInfo
    //{
    //    [JsonProperty("disable_friend_recommend_guide_label")]
    //    public bool DisableFriendRecommendGuideLabel { get; set; }

    //    [JsonProperty("friend_recommend_source")]
    //    public int FriendRecommendSource { get; set; }
    //}

    //public class GameTagInfo
    //{
    //    [JsonProperty("is_game")]
    //    public bool IsGame { get; set; }
    //}

    //public class ImageAlbumMusicInfo
    //{
    //    [JsonProperty("begin_time")]
    //    public int BeginTime { get; set; }

    //    [JsonProperty("end_time")]
    //    public int EndTime { get; set; }

    //    [JsonProperty("volume")]
    //    public int Volume { get; set; }
    //}

    //public class ImageComment
    //{
    //    // 空对象，可根据实际需求补充字段
    //}

    //public class ImpressionData
    //{
    //    [JsonProperty("group_id_list_a")]
    //    public List<object> GroupIdListA { get; set; }

    //    [JsonProperty("group_id_list_b")]
    //    public List<object> GroupIdListB { get; set; }

    //    [JsonProperty("group_id_list_c")]
    //    public List<object> GroupIdListC { get; set; }

    //    [JsonProperty("group_id_list_d")]
    //    public List<object> GroupIdListD { get; set; }

    //    [JsonProperty("similar_id_list_a")]
    //    public object SimilarIdListA { get; set; }

    //    [JsonProperty("similar_id_list_b")]
    //    public List<string> SimilarIdListB { get; set; }
    //}

    //public class ItemWarnNotification
    //{
    //    [JsonProperty("content")]
    //    public string Content { get; set; }

    //    [JsonProperty("show")]
    //    public bool Show { get; set; }

    //    [JsonProperty("type")]
    //    public int Type { get; set; }
    //}

    public class MixInfo
    {
        [JsonProperty("cover_url")]
        public ImageInfo CoverUrl { get; set; }

        //[JsonProperty("create_time")]
        //public long CreateTime { get; set; }

        //[JsonProperty("desc")]
        //public string Desc { get; set; }

        //[JsonProperty("enable_ad")]
        //public int EnableAd { get; set; }

        //[JsonProperty("extra")]
        //public string Extra { get; set; }

        //[JsonProperty("ids")]
        //public object Ids { get; set; }

        //[JsonProperty("is_iaa")]
        //public int IsIaa { get; set; }

        //[JsonProperty("is_serial_mix")]
        //public int IsSerialMix { get; set; }

        //[JsonProperty("mix_id")]
        //public string MixId { get; set; }

        //[JsonProperty("mix_name")]
        //public string MixName { get; set; }

        //[JsonProperty("mix_pic_type")]
        //public int MixPicType { get; set; }

        //[JsonProperty("mix_type")]
        //public int MixType { get; set; }

        //[JsonProperty("share_info")]
        //public MixShareInfo ShareInfo { get; set; }

        [JsonProperty("statis")]
        public MixStatis Statis { get; set; }

        //[JsonProperty("status")]
        //public MixStatus Status { get; set; }

        //[JsonProperty("update_time")]
        //public long UpdateTime { get; set; }

        //[JsonProperty("watched_item")]
        //public string WatchedItem { get; set; }
    }

    //public class MixShareInfo
    //{
    //    [JsonProperty("share_desc")]
    //    public string ShareDesc { get; set; }

    //    [JsonProperty("share_desc_info")]
    //    public string ShareDescInfo { get; set; }

    //    [JsonProperty("share_title")]
    //    public string ShareTitle { get; set; }

    //    [JsonProperty("share_title_myself")]
    //    public string ShareTitleMyself { get; set; }

    //    [JsonProperty("share_title_other")]
    //    public string ShareTitleOther { get; set; }

    //    [JsonProperty("share_url")]
    //    public string ShareUrl { get; set; }

    //    [JsonProperty("share_weibo_desc")]
    //    public string ShareWeiboDesc { get; set; }
    //}

    public class MixStatis
    {
        [JsonProperty("collect_vv")]
        public int CollectVv { get; set; }

        [JsonProperty("current_episode")]
        public int CurrentEpisode { get; set; }

        [JsonProperty("play_vv")]
        public int PlayVv { get; set; }

        [JsonProperty("updated_to_episode")]
        public int UpdatedToEpisode { get; set; }
    }

    //public class MixStatus
    //{
    //    [JsonProperty("is_collected")]
    //    public int IsCollected { get; set; }

    //    [JsonProperty("status")]
    //    public int Status { get; set; }
    //}

    public class Music
    {
        //[JsonProperty("album")]
        //public string Album { get; set; }

        //[JsonProperty("artist_user_infos")]
        //public object ArtistUserInfos { get; set; }

        //[JsonProperty("artists")]
        //public List<object> Artists { get; set; }

        //[JsonProperty("audition_duration")]
        //public int AuditionDuration { get; set; }

        //[JsonProperty("author")]
        //public string Author { get; set; }

        //[JsonProperty("author_deleted")]
        //public bool AuthorDeleted { get; set; }

        //[JsonProperty("author_position")]
        //public object AuthorPosition { get; set; }

        //[JsonProperty("author_status")]
        //public int AuthorStatus { get; set; }

        //[JsonProperty("avatar_large")]
        //public ImageInfo AvatarLarge { get; set; }

        //[JsonProperty("avatar_medium")]
        //public ImageInfo AvatarMedium { get; set; }

        //[JsonProperty("avatar_thumb")]
        //public ImageInfo AvatarThumb { get; set; }

        //[JsonProperty("binded_challenge_id")]
        //public int BindedChallengeId { get; set; }

        //[JsonProperty("can_background_play")]
        //public bool CanBackgroundPlay { get; set; }

        //[JsonProperty("collect_stat")]
        //public int CollectStat { get; set; }

        [JsonProperty("cover_hd")]
        public ImageInfo CoverHd { get; set; }

        //[JsonProperty("cover_large")]
        //public ImageInfo CoverLarge { get; set; }

        //[JsonProperty("cover_medium")]
        //public ImageInfo CoverMedium { get; set; }

        //[JsonProperty("cover_thumb")]
        //public ImageInfo CoverThumb { get; set; }

        //[JsonProperty("dmv_auto_show")]
        //public bool DmvAutoShow { get; set; }

        //[JsonProperty("dsp_status")]
        //public int DspStatus { get; set; }

        //[JsonProperty("duration")]
        //public int Duration { get; set; }

        //[JsonProperty("end_time")]
        //public int EndTime { get; set; }

        //[JsonProperty("external_song_info")]
        //public List<object> ExternalSongInfo { get; set; }

        //[JsonProperty("extra")]
        //public string Extra { get; set; }

        //[JsonProperty("id")]
        //public long Id { get; set; }

        //[JsonProperty("id_str")]
        //public string IdStr { get; set; }

        //[JsonProperty("is_audio_url_with_cookie")]
        //public bool IsAudioUrlWithCookie { get; set; }

        //[JsonProperty("is_commerce_music")]
        //public bool IsCommerceMusic { get; set; }

        //[JsonProperty("is_del_video")]
        //public bool IsDelVideo { get; set; }

        //[JsonProperty("is_matched_metadata")]
        //public bool IsMatchedMetadata { get; set; }

        //[JsonProperty("is_original")]
        //public bool IsOriginal { get; set; }

        //[JsonProperty("is_original_sound")]
        //public bool IsOriginalSound { get; set; }

        //[JsonProperty("is_pgc")]
        //public bool IsPgc { get; set; }

        //[JsonProperty("is_restricted")]
        //public bool IsRestricted { get; set; }

        //[JsonProperty("is_video_self_see")]
        //public bool IsVideoSelfSee { get; set; }

        //[JsonProperty("lyric_short_position")]
        //public object LyricShortPosition { get; set; }

        //[JsonProperty("mid")]
        //public string Mid { get; set; }

        //[JsonProperty("music_chart_ranks")]
        //public object MusicChartRanks { get; set; }

        //[JsonProperty("music_collect_count")]
        //public int MusicCollectCount { get; set; }

        //[JsonProperty("music_cover_atmosphere_color_value")]
        //public string MusicCoverAtmosphereColorValue { get; set; }

        //[JsonProperty("music_status")]
        //public int MusicStatus { get; set; }

        //[JsonProperty("musician_user_infos")]
        //public object MusicianUserInfos { get; set; }

        //[JsonProperty("mute_share")]
        //public bool MuteShare { get; set; }

        //[JsonProperty("offline_desc")]
        //public string OfflineDesc { get; set; }

        //[JsonProperty("owner_handle")]
        //public string OwnerHandle { get; set; }

        //[JsonProperty("owner_id")]
        //public string OwnerId { get; set; }

        //[JsonProperty("owner_nickname")]
        //public string OwnerNickname { get; set; }

        //[JsonProperty("pgc_music_type")]
        //public int PgcMusicType { get; set; }

        [JsonProperty("play_url")]
        public ImageInfo PlayUrl { get; set; }

        //[JsonProperty("position")]
        //public object Position { get; set; }

        //[JsonProperty("prevent_download")]
        //public bool PreventDownload { get; set; }

        //[JsonProperty("prevent_item_download_status")]
        //public int PreventItemDownloadStatus { get; set; }

        //[JsonProperty("preview_end_time")]
        //public int PreviewEndTime { get; set; }

        //[JsonProperty("preview_start_time")]
        //public int PreviewStartTime { get; set; }

        //[JsonProperty("reason_type")]
        //public int ReasonType { get; set; }

        //[JsonProperty("redirect")]
        //public bool Redirect { get; set; }

        //[JsonProperty("schema_url")]
        //public string SchemaUrl { get; set; }

        //[JsonProperty("search_impr")]
        //public SearchImpr SearchImpr { get; set; }

        //[JsonProperty("sec_uid")]
        //public string SecUid { get; set; }

        //[JsonProperty("shoot_duration")]
        //public int ShootDuration { get; set; }

        //[JsonProperty("show_origin_clip")]
        //public bool ShowOriginClip { get; set; }

        //[JsonProperty("source_platform")]
        //public int SourcePlatform { get; set; }

        //[JsonProperty("start_time")]
        //public int StartTime { get; set; }

        //[JsonProperty("status")]
        //public int Status { get; set; }

        //[JsonProperty("strong_beat_url")]
        //public ImageInfo StrongBeatUrl { get; set; }

        //[JsonProperty("tag_list")]
        //public object TagList { get; set; }

        //[JsonProperty("title")]
        //public string Title { get; set; }

        //[JsonProperty("unshelve_countries")]
        //public object UnshelveCountries { get; set; }

        //[JsonProperty("user_count")]
        //public int UserCount { get; set; }

        //[JsonProperty("video_duration")]
        //public int VideoDuration { get; set; }
    }

    //public class SearchImpr
    //{
    //    [JsonProperty("entity_id")]
    //    public string EntityId { get; set; }
    //}

    //public class PhotoSearchEntrance
    //{
    //    [JsonProperty("ecom_type")]
    //    public int EcomType { get; set; }
    //}

    //public class PoiPatchInfo
    //{
    //    [JsonProperty("extra")]
    //    public string Extra { get; set; }

    //    [JsonProperty("item_patch_poi_prompt_mark")]
    //    public int ItemPatchPoiPromptMark { get; set; }
    //}

    //public class PublishPlusAlienation
    //{
    //    [JsonProperty("alienation_type")]
    //    public int AlienationType { get; set; }
    //}

    //public class RelatedVideoExtra
    //{
    //    [JsonProperty("tags")]
    //    public string Tags { get; set; }
    //}

    //public class RiskInfos
    //{
    //    [JsonProperty("content")]
    //    public string Content { get; set; }

    //    [JsonProperty("risk_sink")]
    //    public bool RiskSink { get; set; }

    //    [JsonProperty("type")]
    //    public int Type { get; set; }

    //    [JsonProperty("vote")]
    //    public bool Vote { get; set; }

    //    [JsonProperty("warn")]
    //    public bool Warn { get; set; }
    //}

    //public class SeriesBasicInfo
    //{
    //    // 空对象，可根据实际需求补充字段
    //}

    //public class SeriesPaidInfo
    //{
    //    [JsonProperty("item_price")]
    //    public int ItemPrice { get; set; }

    //    [JsonProperty("series_paid_status")]
    //    public int SeriesPaidStatus { get; set; }
    //}

    //public class ShareInfo
    //{
    //    [JsonProperty("share_desc_info")]
    //    public string ShareDescInfo { get; set; }

    //    [JsonProperty("share_link_desc")]
    //    public string ShareLinkDesc { get; set; }

    //    [JsonProperty("share_url")]
    //    public string ShareUrl { get; set; }
    //}

    //public class Statistics
    //{
    //    [JsonProperty("collect_count")]
    //    public int CollectCount { get; set; }

    //    [JsonProperty("comment_count")]
    //    public int CommentCount { get; set; }

    //    [JsonProperty("digest")]
    //    public string Digest { get; set; }

    //    [JsonProperty("digg_count")]
    //    public int DiggCount { get; set; }

    //    [JsonProperty("exposure_count")]
    //    public int ExposureCount { get; set; }

    //    [JsonProperty("live_watch_count")]
    //    public int LiveWatchCount { get; set; }

    //    [JsonProperty("play_count")]
    //    public int PlayCount { get; set; }

    //    [JsonProperty("share_count")]
    //    public int ShareCount { get; set; }
    //}

    //public class Status
    //{
    //    [JsonProperty("allow_share")]
    //    public bool AllowShare { get; set; }

    //    [JsonProperty("aweme_edit_info")]
    //    public AwemeEditInfo AwemeEditInfo { get; set; }

    //    [JsonProperty("dont_share_status")]
    //    public int DontShareStatus { get; set; }

    //    [JsonProperty("enable_soft_delete")]
    //    public int EnableSoftDelete { get; set; }

    //    [JsonProperty("in_reviewing")]
    //    public bool InReviewing { get; set; }

    //    [JsonProperty("is_delete")]
    //    public bool IsDelete { get; set; }

    //    [JsonProperty("is_prohibited")]
    //    public bool IsProhibited { get; set; }

    //    [JsonProperty("listen_video_status")]
    //    public int ListenVideoStatus { get; set; }

    //    [JsonProperty("not_allow_soft_del_reason")]
    //    public string NotAllowSoftDelReason { get; set; }

    //    [JsonProperty("part_see")]
    //    public int PartSee { get; set; }

    //    [JsonProperty("private_status")]
    //    public int PrivateStatus { get; set; }

    //    [JsonProperty("review_result")]
    //    public ReviewResult ReviewResult { get; set; }

    //    [JsonProperty("video_hide_search")]
    //    public int VideoHideSearch { get; set; }
    //}

    //public class AwemeEditInfo
    //{
    //    [JsonProperty("button_status")]
    //    public int ButtonStatus { get; set; }

    //    [JsonProperty("button_toast")]
    //    public string ButtonToast { get; set; }

    //    [JsonProperty("edit_status")]
    //    public int EditStatus { get; set; }

    //    [JsonProperty("has_modified_all")]
    //    public bool HasModifiedAll { get; set; }
    //}

    //public class ReviewResult
    //{
    //    [JsonProperty("review_status")]
    //    public int ReviewStatus { get; set; }
    //}

    //public class TextExtra
    //{
    //    [JsonProperty("caption_end")]
    //    public int CaptionEnd { get; set; }

    //    [JsonProperty("caption_start")]
    //    public int CaptionStart { get; set; }

    //    [JsonProperty("end")]
    //    public int End { get; set; }

    //    [JsonProperty("hashtag_id")]
    //    public string HashtagId { get; set; }

    //    [JsonProperty("hashtag_name")]
    //    public string HashtagName { get; set; }

    //    [JsonProperty("is_commerce")]
    //    public bool IsCommerce { get; set; }

    //    [JsonProperty("start")]
    //    public int Start { get; set; }

    //    [JsonProperty("type")]
    //    public int Type { get; set; }
    //}

    public class Video
    {
        //[JsonProperty("audio")]
        //public Audio Audio { get; set; }

        //[JsonProperty("big_thumbs")]
        //public List<BigThumb> BigThumbs { get; set; }

        [JsonProperty("bit_rate")]
        public List<VideoBitRate> BitRate { get; set; }


        // 可能存在的其他封面相关字段（补充完整）
        [JsonProperty("cover")]
        public ImageInfo Cover { get; set; } // 静态封面
    }


    //public class BigThumb
    //{
    //    [JsonProperty("duration")]
    //    public double Duration { get; set; }

    //    [JsonProperty("fext")]
    //    public string Fext { get; set; }

    //    [JsonProperty("img_num")]
    //    public int ImgNum { get; set; }

    //    [JsonProperty("img_url")]
    //    public string ImgUrl { get; set; }

    //    [JsonProperty("img_urls")]
    //    public List<object> ImgUrls { get; set; }

    //    [JsonProperty("img_x_len")]
    //    public int ImgXLen { get; set; }

    //    [JsonProperty("img_x_size")]
    //    public int ImgXSize { get; set; }

    //    [JsonProperty("img_y_len")]
    //    public int ImgYLen { get; set; }

    //    [JsonProperty("img_y_size")]
    //    public int ImgYSize { get; set; }

    //    [JsonProperty("interval")]
    //    public double Interval { get; set; }

    //    [JsonProperty("uri")]
    //    public string Uri { get; set; }

    //    [JsonProperty("uris")]
    //    public List<object> Uris { get; set; }
    //}

    public class VideoBitRate
    {
        [JsonProperty("FPS")]
        public int Fps { get; set; }

        [JsonProperty("HDR_bit")]
        public string HdrBit { get; set; }

        [JsonProperty("HDR_type")]
        public string HdrType { get; set; }

        [JsonProperty("bit_rate")]
        public int BitRateValue { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        //[JsonProperty("gear_name")]
        //public string GearName { get; set; }

        //[JsonProperty("is_bytevc1")]
        //public int IsBytevc1 { get; set; }

        [JsonProperty("is_h265")]
        public int IsH265 { get; set; }

        [JsonProperty("play_addr")]
        public PlayAddr PlayAddr { get; set; }

        [JsonProperty("quality_type")]
        public int QualityType { get; set; }

        //[JsonProperty("video_extra")]
        //public string VideoExtra { get; set; }
    }

    /// <summary>
    /// 取第一个（质量最好）
    /// </summary>
    public class PlayAddr
    {
        [JsonProperty("data_size")]
        public long DataSize { get; set; }

        //[JsonProperty("file_cs")]
        //public string FileCs { get; set; }

        [JsonProperty("file_hash")]
        public string FileHash { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        //[JsonProperty("uri")]
        //public string Uri { get; set; }

        //[JsonProperty("url_key")]
        //public string UrlKey { get; set; }

        [JsonProperty("url_list")]
        public List<string> UrlList { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }



    public class ImageItemInfo
    {
        /// <summary>
        /// 
        /// </summary>
        //public List<string> download_url_list { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public string uri { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("url_list")]
        public List<string> UrlList { get; set; }


        [JsonProperty("video")]
        public Video DynamicVideo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }
    }



}
