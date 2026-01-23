using Newtonsoft.Json;

namespace dy.net.model.response
{
    /// <summary>
    /// 抖音短剧收藏返回结果根对象
    /// </summary>
    public class DouyinSeriesListResponse
    {
        [JsonProperty("cursor")]
        public int Cursor { get; set; }

        //[JsonProperty("extra")]
        //public DouyinResponseExtra Extra { get; set; }

        [JsonProperty("has_more")]
        public int HasMore { get; set; }

        //[JsonProperty("log_pb")]
        //public DouyinResponseLog Log { get; set; }

        //[JsonProperty("series_info")]
        //public object ShortDramaSingleInfo { get; set; } // 原始值为null，兼容扩展

        [JsonProperty("series_infos")]
        public List<DouyinSeriesInfo> SeriesList { get; set; }

        [JsonProperty("status_code")]
        public int ResponseStatusCode { get; set; }
    }

    /// <summary>
    /// 抖音响应额外信息
    /// </summary>
    //public class DouyinResponseExtra
    //{
    //    [JsonProperty("fatal_item_ids")]
    //    public List<object> FatalItemIds { get; set; }

    //    [JsonProperty("logid")]
    //    public string LogId { get; set; }

    //    [JsonProperty("now")]
    //    public long CurrentTimestamp { get; set; }
    //}

    ///// <summary>
    ///// 抖音响应日志信息
    ///// </summary>
    //public class DouyinResponseLog
    //{
    //    [JsonProperty("impr_id")]
    //    public string ImpressionId { get; set; }
    //}

    #region 核心短剧信息
    /// <summary>
    /// 抖音短剧核心信息
    /// </summary>
    public class DouyinSeriesInfo
    {
        [JsonProperty("actors")]
        public List<object> Actors { get; set; }

        [JsonProperty("author")]
        public DouyinShortDramaAuthor Author { get; set; }

        [JsonProperty("content_sub_type")]
        public int ContentSubType { get; set; }

        [JsonProperty("cover_url")]
        public DouyinImageResource CoverImage { get; set; }

        [JsonProperty("create_time")]
        public long CreateTimestamp { get; set; }

        //[JsonProperty("dark_icon_url")]
        //public DouyinImageResource DarkModeIcon { get; set; }

        [JsonProperty("desc")]
        public string SeriesDescription { get; set; }

        //[JsonProperty("directors")]
        //public List<object> Directors { get; set; }

        //[JsonProperty("dog_card_info")]
        //public DouyinDramaRankCard RankCardInfo { get; set; }

        [JsonProperty("enable_use_new_ent_data")]
        public bool EnableNewEntData { get; set; }

        //[JsonProperty("extra")]
        //public string ExtraJson { get; set; } // 嵌套JSON字符串，暂存为string，可二次反序列化

        //[JsonProperty("horizontal_cover_url")]
        //public DouyinImageResource HorizontalCoverImage { get; set; }

        //[JsonProperty("ids")]
        //public List<object> RelatedIds { get; set; }

        //[JsonProperty("is_charge_series")]
        //public int IsPaidDrama { get; set; }

        //[JsonProperty("is_exclusive")]
        //public bool IsExclusiveDrama { get; set; }

        //[JsonProperty("is_iaa")]
        //public int IsIaaDrama { get; set; }

        //[JsonProperty("light_icon_url")]
        //public DouyinImageResource LightModeIcon { get; set; }

        //[JsonProperty("paid_episodes")]
        //public object PaidEpisodes { get; set; } // 原始值为null，兼容扩展
        /// <summary>
        /// 短剧剧名
        /// </summary>
        //[JsonProperty("real_name")]
        //public string DramaRealName { get; set; }

        //[JsonProperty("recommend_color")]
        //public object RecommendColor { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("rights_info")]
        //public DouyinDramaRightsInfo RightsInfo { get; set; }

        //[JsonProperty("series_content_types")]
        //public List<DouyinDramaContentType> ContentTypes { get; set; }

        //[JsonProperty("series_content_types_new")]
        //public List<DouyinDramaNewContentType> NewContentTypes { get; set; }

        //[JsonProperty("series_form_type")]
        //public int DramaFormType { get; set; }

        //[JsonProperty("series_icp")]
        //public DouyinDramaIcpInfo IcpRecordInfo { get; set; }
        /// <summary>
        /// 短剧ID
        /// </summary>
        [JsonProperty("series_id")]
        public string SeriesId { get; set; }

        //[JsonProperty("series_interactive")]
        //public DouyinDramaInteractiveConfig InteractiveConfig { get; set; }
        /// <summary>
        /// 短剧名称
        /// </summary>
        [JsonProperty("series_name")]
        public string SeriesName { get; set; }

        //[JsonProperty("series_paid_type_list")]
        //public object PaidTypeList { get; set; } // 原始值为null，兼容扩展

        [JsonProperty("series_price")]
        public int SeriesPrice { get; set; }

        //[JsonProperty("series_rank_info")]
        //public DouyinDramaRankInfo RankInfo { get; set; }

        //[JsonProperty("series_type")]
        //public int DramaType { get; set; }

        //[JsonProperty("series_ui_config")]
        //public DouyinDramaUiConfig UiConfig { get; set; }

        //[JsonProperty("share_info")]
        //public DouyinDramaShareInfo ShareInfo { get; set; }

        [JsonProperty("stats")]
        public DouyinDramaStats Stats { get; set; }

        //[JsonProperty("status")]
        //public DouyinDramaStatus DramaStatus { get; set; }

        [JsonProperty("update_time")]
        public long UpdateTimestamp { get; set; }

        [JsonProperty("watched_episode")]
        public int WatchedEpisodeCount { get; set; }

        [JsonProperty("watched_item")]
        public string WatchedItemId { get; set; }

        [JsonProperty("watched_time")]
        public long WatchedTimestamp { get; set; }
    }
    #endregion

    #region 作者信息
    /// <summary>
    /// 抖音短剧作者信息
    /// </summary>
    public class DouyinShortDramaAuthor
    {
        //[JsonProperty("accept_private_policy")]
        //public bool AcceptPrivatePolicy { get; set; }

        //[JsonProperty("account_region")]
        //public string AccountRegion { get; set; }

        //[JsonProperty("ad_cover_url")]
        //public object AdCoverUrl { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("allow_story_normal_like")]
        //public bool AllowStoryNormalLike { get; set; }

        //[JsonProperty("apple_account")]
        //public int AppleAccount { get; set; }

        //[JsonProperty("authority_status")]
        //public int AuthorityStatus { get; set; }

        //[JsonProperty("avatar_168x168")]
        //public DouyinImageResource Avatar168x168 { get; set; }

        //[JsonProperty("avatar_300x300")]
        //public DouyinImageResource Avatar300x300 { get; set; }

        //[JsonProperty("avatar_larger")]
        //public DouyinImageResource AvatarLarger { get; set; }



        //[JsonProperty("avatar_schema_list")]
        //public object AvatarSchemaList { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("avatar_thumb")]
        //public DouyinImageResource AvatarThumb { get; set; }

        //[JsonProperty("avatar_uri")]
        //public string AvatarUri { get; set; }

        //[JsonProperty("aweme_control")]
        //public DouyinAuthorContentControl ContentControl { get; set; }

        //[JsonProperty("aweme_count")]
        //public int PublishedContentCount { get; set; }

        //[JsonProperty("aweme_hotsoon_auth")]
        //public int HotsoonAuth { get; set; }

        //[JsonProperty("aweme_hotsoon_auth_relation")]
        //public int HotsoonAuthRelation { get; set; }

        //[JsonProperty("awemehts_greet_info")]
        //public string HotsoonGreetInfo { get; set; }

        //[JsonProperty("ban_user_functions")]
        //public List<object> BannedFunctions { get; set; }

        //[JsonProperty("batch_unfollow_contain_tabs")]
        //public object BatchUnfollowTabs { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("batch_unfollow_relation_desc")]
        //public object BatchUnfollowDesc { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("can_set_geofencing")]
        //public object CanSetGeofencing { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("card_entries")]
        //public object CardEntries { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("card_entries_not_display")]
        //public object HiddenCardEntries { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("card_sort_priority")]
        //public object CardSortPriority { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("cf_list")]
        //public object CfList { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("cha_list")]
        //public object ChaList { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("close_friend_type")]
        //public int CloseFriendType { get; set; }

        //[JsonProperty("comment_filter_status")]
        //public int CommentFilterStatus { get; set; }

        //[JsonProperty("comment_setting")]
        //public int CommentSetting { get; set; }

        //[JsonProperty("commerce_user_level")]
        //public int CommerceUserLevel { get; set; }

        //[JsonProperty("constellation")]
        //public int Constellation { get; set; }

        //[JsonProperty("contacts_status")]
        //public int ContactsStatus { get; set; }

        //[JsonProperty("contrail_list")]
        //public object ContrailList { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("cover_url")]
        //public List<DouyinImageResource> AuthorCoverImages { get; set; }

        //[JsonProperty("create_time")]
        //public long AccountCreateTimestamp { get; set; }

        //[JsonProperty("creator_tag_list")]
        //public object CreatorTags { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("custom_verify")]
        //public string CustomVerify { get; set; }

        //[JsonProperty("cv_level")]
        //public string CvLevel { get; set; }

        //[JsonProperty("data_label_list")]
        //public object DataLabels { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("disable_image_comment_saved")]
        //public int DisableImageCommentSaved { get; set; }

        //[JsonProperty("display_info")]
        //public object DisplayInfo { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("download_prompt_ts")]
        //public long DownloadPromptTimestamp { get; set; }

        //[JsonProperty("download_setting")]
        //public int DownloadSetting { get; set; }

        //[JsonProperty("duet_setting")]
        //public int DuetSetting { get; set; }

        //[JsonProperty("enable_nearby_visible")]
        //public bool EnableNearbyVisible { get; set; }

        //[JsonProperty("endorsement_info_list")]
        //public object EndorsementInfos { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("enterprise_verify_reason")]
        //public string EnterpriseVerifyReason { get; set; }

        //[JsonProperty("familiar_confidence")]
        //public int FamiliarConfidence { get; set; }

        //[JsonProperty("familiar_visitor_user")]
        //public object FamiliarVisitor { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("favoriting_count")]
        //public int FavoritedCount { get; set; }

        //[JsonProperty("fb_expire_time")]
        //public long FacebookTokenExpireTimestamp { get; set; }

        //[JsonProperty("follow_status")]
        //public int FollowStatus { get; set; }

        //[JsonProperty("follower_count")]
        //public int FollowerCount { get; set; }

        //[JsonProperty("follower_list_secondary_information_struct")]
        //public object FollowerSecondaryInfo { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("follower_request_status")]
        //public int FollowerRequestStatus { get; set; }

        //[JsonProperty("follower_status")]
        //public int FollowerStatus { get; set; }

        //[JsonProperty("followers_detail")]
        //public object FollowerDetail { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("following_count")]
        //public int FollowingCount { get; set; }

        //[JsonProperty("geofencing")]
        //public List<object> GeofencingList { get; set; }

        //[JsonProperty("google_account")]
        //public string GoogleAccount { get; set; }

        //[JsonProperty("has_email")]
        //public bool HasBindEmail { get; set; }

        //[JsonProperty("has_facebook_token")]
        //public bool HasFacebookToken { get; set; }

        //[JsonProperty("has_insights")]
        //public bool HasInsights { get; set; }

        //[JsonProperty("has_orders")]
        //public bool HasCommerceOrders { get; set; }

        //[JsonProperty("has_twitter_token")]
        //public bool HasTwitterToken { get; set; }

        //[JsonProperty("has_unread_story")]
        //public bool HasUnreadStory { get; set; }

        //[JsonProperty("has_youtube_token")]
        //public bool HasYoutubeToken { get; set; }

        //[JsonProperty("hide_location")]
        //public bool HideLocation { get; set; }

        //[JsonProperty("hide_search")]
        //public bool HideSearch { get; set; }

        //[JsonProperty("homepage_bottom_toast")]
        //public object HomepageBottomToast { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("house_talent_info")]
        //public int HouseTalentInfo { get; set; }

        //[JsonProperty("identity_labels")]
        //public object IdentityLabels { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("im_role_ids")]
        //public object ImRoleIds { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("ins_id")]
        //public string InstagramId { get; set; }

        //[JsonProperty("interest_tags")]
        //public object InterestTags { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("is_ad_fake")]
        //public bool IsAdFakeAccount { get; set; }

        //[JsonProperty("is_ban")]
        //public bool IsAccountBanned { get; set; }

        //[JsonProperty("is_binded_weibo")]
        //public bool HasBindWeibo { get; set; }

        //[JsonProperty("is_block")]
        //public bool IsBlocked { get; set; }

        //[JsonProperty("is_blocked_v2")]
        //public bool IsBlockedV2 { get; set; }

        //[JsonProperty("is_blocking_v2")]
        //public bool IsBlockingOthersV2 { get; set; }

        //[JsonProperty("is_cf")]
        //public int IsCfAccount { get; set; }

        //[JsonProperty("is_discipline_member")]
        //public bool IsDisciplineMember { get; set; }

        //[JsonProperty("is_gov_media_vip")]
        //public bool IsGovMediaVip { get; set; }

        //[JsonProperty("is_mix_user")]
        //public bool IsMixUser { get; set; }

        //[JsonProperty("is_not_show")]
        //public bool IsNotShow { get; set; }

        //[JsonProperty("is_phone_binded")]
        //public bool HasBindPhone { get; set; }

        //[JsonProperty("is_star")]
        //public bool IsStarAccount { get; set; }

        //[JsonProperty("is_verified")]
        //public bool IsVerifiedAccount { get; set; }

        //[JsonProperty("item_list")]
        //public object PublishedItemList { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("ky_only_predict")]
        //public int KyOnlyPredict { get; set; }

        //[JsonProperty("language")]
        //public string AccountLanguage { get; set; }

        //[JsonProperty("link_item_list")]
        //public object LinkedItemList { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("live_agreement")]
        //public int LiveAgreement { get; set; }

        //[JsonProperty("live_agreement_time")]
        //public long LiveAgreementTimestamp { get; set; }

        //[JsonProperty("live_commerce")]
        //public bool IsLiveCommerceEnabled { get; set; }

        //[JsonProperty("live_high_value")]
        //public int LiveHighValue { get; set; }

        //[JsonProperty("live_status")]
        //public int CurrentLiveStatus { get; set; }

        //[JsonProperty("live_verify")]
        //public int LiveVerifyStatus { get; set; }

        //[JsonProperty("location")]
        //public string AccountLocation { get; set; }

        //[JsonProperty("mate_add_permission")]
        //public int MateAddPermission { get; set; }

        //[JsonProperty("mate_count")]
        //public int MateCount { get; set; }

        //[JsonProperty("mate_relation")]
        //public DouyinAuthorMateRelation MateRelation { get; set; }

        //[JsonProperty("max_follower_count")]
        //public int MaxFollowerCount { get; set; }

        //[JsonProperty("need_points")]
        //public object NeedPoints { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("need_recommend")]
        //public int NeedRecommend { get; set; }

        //[JsonProperty("neiguang_shield")]
        //public int NeiguangShield { get; set; }

        //[JsonProperty("new_friend_type")]
        //public int NewFriendType { get; set; }

        //[JsonProperty("new_story_cover")]
        //public object NewStoryCover { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("not_seen_item_id_list")]
        //public object UnseenItemIds { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("not_seen_item_id_list_v2")]
        //public object UnseenItemIdsV2 { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("offline_info_list")]
        //public object OfflineInfos { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("personal_tag_list")]
        //public object PersonalTags { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("platform_sync_info")]
        //public object PlatformSyncInfo { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("prevent_download")]
        //public bool PreventContentDownload { get; set; }

        //[JsonProperty("private_relation_list")]
        //public object PrivateRelationList { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("profile_component_disabled")]
        //public object DisabledProfileComponents { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("profile_mob_params")]
        //public object MobileProfileParams { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("react_setting")]
        //public int ReactSetting { get; set; }

        //[JsonProperty("reflow_page_gid")]
        //public int ReflowPageGid { get; set; }

        //[JsonProperty("reflow_page_uid")]
        //public int ReflowPageUid { get; set; }

        //[JsonProperty("region")]
        //public string Region { get; set; }

        //[JsonProperty("relative_users")]
        //public object RelativeUsers { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("risk_notice_text")]
        //public string RiskNoticeText { get; set; }

        //[JsonProperty("room_id")]
        //public long LiveRoomId { get; set; }

        //[JsonProperty("room_id_str")]
        //public string LiveRoomIdStr { get; set; }

        //[JsonProperty("school_category")]
        //public int SchoolCategory { get; set; }

        //[JsonProperty("school_id")]
        //public string SchoolId { get; set; }

        //[JsonProperty("school_name")]
        //public string SchoolName { get; set; }

        //[JsonProperty("school_poi_id")]
        //public string SchoolPoiId { get; set; }

        //[JsonProperty("school_type")]
        //public int SchoolType { get; set; }

        //[JsonProperty("search_impr")]
        //public DouyinAuthorSearchImpr SearchImpr { get; set; }
        /// <summary>
        /// 作者sec_uid
        /// </summary>
        [JsonProperty("sec_uid")]
        public string SecureUid { get; set; }

        /// <summary>
        /// 作者名称
        /// </summary>
        [JsonProperty("nickname")]
        public string AuthorNickname { get; set; }
        /// <summary>
        /// 作者头像信息
        /// </summary>
        [JsonProperty("avatar_medium")]
        public DouyinImageResource AvatarMedium { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("short_id")]
        public string ShortUid { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        //[JsonProperty("unique_id")]
        //public string UniqueAccountId { get; set; }

        //[JsonProperty("secret")]
        //public int SecretLevel { get; set; }

        //[JsonProperty("shield_comment_notice")]
        //public int ShieldCommentNotice { get; set; }

        //[JsonProperty("shield_digg_notice")]
        //public int ShieldDiggNotice { get; set; }

        //[JsonProperty("shield_follow_notice")]
        //public int ShieldFollowNotice { get; set; }



        //[JsonProperty("show_image_bubble")]
        //public bool ShowImageBubble { get; set; }

        //[JsonProperty("show_nearby_active")]
        //public bool ShowNearbyActive { get; set; }

        //[JsonProperty("signature")]
        //public string AuthorSignature { get; set; }

        //[JsonProperty("signature_display_lines")]
        //public int SignatureDisplayLines { get; set; }

        //[JsonProperty("signature_extra")]
        //public object SignatureExtra { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("social_real_relation_type")]
        //public int SocialRealRelationType { get; set; }

        //[JsonProperty("special_follow_status")]
        //public int SpecialFollowStatus { get; set; }

        //[JsonProperty("special_lock")]
        //public int SpecialLock { get; set; }

        //[JsonProperty("special_people_labels")]
        //public object SpecialPeopleLabels { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("status")]
        //public int AccountStatus { get; set; }

        //[JsonProperty("stitch_setting")]
        //public int StitchSetting { get; set; }

        //[JsonProperty("story25_comment")]
        //public int Story25Comment { get; set; }

        //[JsonProperty("story_count")]
        //public int StoryCount { get; set; }

        //[JsonProperty("story_interactive")]
        //public int StoryInteractive { get; set; }

        //[JsonProperty("story_open")]
        //public bool IsStoryOpen { get; set; }

        //[JsonProperty("story_ttl")]
        //public long StoryTtl { get; set; }

        //[JsonProperty("sync_to_toutiao")]
        //public int SyncToToutiao { get; set; }

        //[JsonProperty("text_extra")]
        //public object TextExtra { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("total_favorited")]
        //public int TotalFavoritedCount { get; set; }

        //[JsonProperty("tw_expire_time")]
        //public long TwitterTokenExpireTimestamp { get; set; }

        //[JsonProperty("twitter_id")]
        //public string TwitterId { get; set; }

        //[JsonProperty("twitter_name")]
        //public string TwitterName { get; set; }

        //[JsonProperty("type_label")]
        //public object TypeLabel { get; set; } // 原始值为null，兼容扩展



        //[JsonProperty("unique_id_modify_time")]
        //public long UniqueIdModifyTimestamp { get; set; }

        //[JsonProperty("user_canceled")]
        //public bool IsAccountCanceled { get; set; }

        //[JsonProperty("user_mode")]
        //public int UserMode { get; set; }

        //[JsonProperty("user_not_see")]
        //public int UserNotSee { get; set; }

        //[JsonProperty("user_not_show")]
        //public int UserNotShow { get; set; }

        //[JsonProperty("user_period")]
        //public int UserPeriod { get; set; }

        //[JsonProperty("user_permissions")]
        //public object UserPermissions { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("user_rate")]
        //public int UserRate { get; set; }

        //[JsonProperty("user_tags")]
        //public object UserTags { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("verification_permission_ids")]
        //public object VerificationPermissionIds { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("verification_type")]
        //public int VerificationType { get; set; }

        //[JsonProperty("verify_info")]
        //public string VerifyInfo { get; set; }

        //[JsonProperty("video_icon")]
        //public DouyinImageResource VideoIcon { get; set; }

        //[JsonProperty("weibo_name")]
        //public string WeiboName { get; set; }

        //[JsonProperty("weibo_schema")]
        //public string WeiboSchema { get; set; }

        //[JsonProperty("weibo_url")]
        //public string WeiboUrl { get; set; }

        //[JsonProperty("weibo_verify")]
        //public string WeiboVerify { get; set; }

        //[JsonProperty("white_cover_url")]
        //public object WhiteCoverUrl { get; set; } // 原始值为null，兼容扩展

        //[JsonProperty("with_commerce_entry")]
        //public bool HasCommerceEntry { get; set; }

        //[JsonProperty("with_dou_entry")]
        //public bool HasDouEntry { get; set; }

        //[JsonProperty("with_fusion_shop_entry")]
        //public bool HasFusionShopEntry { get; set; }

        //[JsonProperty("with_shop_entry")]
        //public bool HasShopEntry { get; set; }

        //[JsonProperty("youtube_channel_id")]
        //public string YoutubeChannelId { get; set; }

        //[JsonProperty("youtube_channel_title")]
        //public string YoutubeChannelTitle { get; set; }

        //[JsonProperty("youtube_expire_time")]
        //public long YoutubeTokenExpireTimestamp { get; set; }
    }

    /// <summary>
    /// 抖音作者内容控制配置
    /// </summary>
    //public class DouyinAuthorContentControl
    //{
    //    [JsonProperty("can_comment")]
    //    public bool AllowComment { get; set; }

    //    [JsonProperty("can_forward")]
    //    public bool AllowForward { get; set; }

    //    [JsonProperty("can_share")]
    //    public bool AllowShare { get; set; }

    //    [JsonProperty("can_show_comment")]
    //    public bool AllowShowComment { get; set; }
    //}

    ///// <summary>
    ///// 抖音作者伴侣关系
    ///// </summary>
    //public class DouyinAuthorMateRelation
    //{
    //    [JsonProperty("mate_apply_forward")]
    //    public int ForwardMateApply { get; set; }

    //    [JsonProperty("mate_apply_reverse")]
    //    public int ReverseMateApply { get; set; }

    //    [JsonProperty("mate_status")]
    //    public int MateStatus { get; set; }
    //}

    /// <summary>
    /// 抖音作者搜索印记
    /// </summary>
    //public class DouyinAuthorSearchImpr
    //{
    //    [JsonProperty("entity_id")]
    //    public string EntityId { get; set; }
    //}
    #endregion

    #region 通用复用实体
    /// <summary>
    /// 抖音图片资源信息（复用：封面、头像、图标等）
    /// </summary>
    public class DouyinImageResource
    {
        [JsonProperty("height")]
        public int ImageHeight { get; set; }

        [JsonProperty("uri")]
        public string ImageUri { get; set; }

        [JsonProperty("url_list")]
        public List<string> ImageUrlList { get; set; }

        [JsonProperty("width")]
        public int ImageWidth { get; set; }
    }

    /// <summary>
    /// 抖音位置信息（复用：ICP备案位置等）
    /// </summary>
    public class DouyinPositionInfo
    {
        [JsonProperty("x_pos")]
        public int XPosition { get; set; }

        [JsonProperty("y_pos")]
        public int YPosition { get; set; }
    }
    #endregion

    #region 短剧辅助信息
    /// <summary>
    /// 抖音短剧排名卡片信息
    /// </summary>
    public class DouyinDramaRankCard
    {
        [JsonProperty("dog_card_text")]
        public string RankCardText { get; set; }

        [JsonProperty("rank_schema")]
        public string RankJumpSchema { get; set; }

        [JsonProperty("rank_type")]
        public string RankType { get; set; }
    }

    /// <summary>
    /// 抖音短剧版权信息
    /// </summary>
    //public class DouyinDramaRightsInfo
    //{
    //    [JsonProperty("has_paid")]
    //    public bool HasPaidRights { get; set; }
    //}

    ///// <summary>
    ///// 抖音短剧内容类型
    ///// </summary>
    //public class DouyinDramaContentType
    //{
    //    [JsonProperty("name")]
    //    public string TypeName { get; set; }

    //    [JsonProperty("series_content_type")]
    //    public int TypeValue { get; set; }
    //}

    ///// <summary>
    ///// 抖音短剧新版内容类型
    ///// </summary>
    //public class DouyinDramaNewContentType
    //{
    //    [JsonProperty("jump_schema")]
    //    public string JumpSchema { get; set; }

    //    [JsonProperty("name")]
    //    public string TypeName { get; set; }

    //    [JsonProperty("series_content_type")]
    //    public int TypeValue { get; set; }
    //}

    ///// <summary>
    ///// 抖音短剧ICP备案信息
    ///// </summary>
    //public class DouyinDramaIcpInfo
    //{
    //    [JsonProperty("card_font_size")]
    //    public int CardFontSize { get; set; }

    //    [JsonProperty("card_pos")]
    //    public DouyinPositionInfo CardPosition { get; set; }

    //    [JsonProperty("disappear_time_sec")]
    //    public int DisappearTimeSeconds { get; set; }

    //    [JsonProperty("font_color")]
    //    public string FontColor { get; set; }

    //    [JsonProperty("font_size")]
    //    public int FontSize { get; set; }

    //    [JsonProperty("horizonal_pos")]
    //    public DouyinPositionInfo HorizontalPosition { get; set; }

    //    [JsonProperty("icp_number")]
    //    public string IcpRecordNumber { get; set; }

    //    [JsonProperty("icp_number_split")]
    //    public string IcpRecordNumberSplit { get; set; }

    //    [JsonProperty("start_time_sec")]
    //    public int ShowStartTimeSeconds { get; set; }

    //    [JsonProperty("vertical_pos")]
    //    public DouyinPositionInfo VerticalPosition { get; set; }
    //}

    ///// <summary>
    ///// 抖音短剧交互配置
    ///// </summary>
    //public class DouyinDramaInteractiveConfig
    //{
    //    [JsonProperty("enable_config")]
    //    public bool EnableInteractiveConfig { get; set; }

    //    [JsonProperty("interactive_config")]
    //    public DouyinDramaInteractiveDetailConfig DetailConfig { get; set; }
    //}

    /// <summary>
    /// 抖音短剧交互详情配置
    /// </summary>
    //public class DouyinDramaInteractiveDetailConfig
    //{
    //    [JsonProperty("collection_button_copy")]
    //    public string CollectButtonText { get; set; }

    //    [JsonProperty("display_detail_edit_button")]
    //    public bool DisplayDetailEditButton { get; set; }

    //    [JsonProperty("hide_desk_guide")]
    //    public bool HideDesktopGuide { get; set; }

    //    [JsonProperty("hide_find_top_tab")]
    //    public bool HideFindTopTab { get; set; }

    //    [JsonProperty("hide_intro_card")]
    //    public bool HideIntroCard { get; set; }

    //    [JsonProperty("hide_intro_card_details_module")]
    //    public bool HideIntroCardDetails { get; set; }

    //    [JsonProperty("hide_intro_card_tags")]
    //    public bool HideIntroCardTags { get; set; }

    //    [JsonProperty("hide_more_series_bottom_btn")]
    //    public bool HideMoreDramaBottomButton { get; set; }

    //    [JsonProperty("hide_more_series_module")]
    //    public bool HideMoreDramaModule { get; set; }

    //    [JsonProperty("hide_recommendation_module")]
    //    public bool HideRecommendationModule { get; set; }

    //    [JsonProperty("more_series_module_copy")]
    //    public string MoreDramaModuleText { get; set; }

    //    [JsonProperty("recommendation_module_title_copy")]
    //    public string RecommendationModuleTitle { get; set; }

    //    [JsonProperty("unlock_button_copy")]
    //    public string UnlockAllButtonText { get; set; }
    //}

    /// <summary>
    /// 抖音短剧排名信息
    /// </summary>
    //public class DouyinDramaRankInfo
    //{
    //    [JsonProperty("rank_card_text")]
    //    public string RankCardText { get; set; }

    //    [JsonProperty("rank_schema")]
    //    public string RankJumpSchema { get; set; }

    //    [JsonProperty("rank_type")]
    //    public string RankType { get; set; }
    //}

    /// <summary>
    /// 抖音短剧UI配置
    /// </summary>
    //public class DouyinDramaUiConfig
    //{
    //    [JsonProperty("collection_button")]
    //    public DouyinDramaCollectButtonConfig CollectButton { get; set; }

    //    [JsonProperty("general_position_tag_infos")]
    //    public object GeneralPositionTags { get; set; } // 原始值为null，兼容扩展

    //    [JsonProperty("paid_series_vip_entrance_config")]
    //    public DouyinDramaVipEntranceConfig VipEntranceConfig { get; set; }

    //    [JsonProperty("series_ad_page_entrance_config")]
    //    public object AdPageEntranceConfig { get; set; } // 原始值为null，兼容扩展
    //}

    /// <summary>
    /// 抖音短剧收藏按钮配置
    /// </summary>
    //public class DouyinDramaCollectButtonConfig
    //{
    //    [JsonProperty("text")]
    //    public string ButtonText { get; set; }
    //}

    ///// <summary>
    ///// 抖音短剧VIP入口配置
    ///// </summary>
    //public class DouyinDramaVipEntranceConfig
    //{
    //    [JsonProperty("block_ad_buttons")]
    //    public List<object> BlockAdButtons { get; set; }

    //    [JsonProperty("unblock_ad_buttons")]
    //    public List<object> UnblockAdButtons { get; set; }

    //    [JsonProperty("upper_right_buttons")]
    //    public List<object> UpperRightButtons { get; set; }
    //}

    ///// <summary>
    ///// 抖音短剧分享信息
    ///// </summary>
    //public class DouyinDramaShareInfo
    //{
    //    [JsonProperty("share_desc")]
    //    public string ShareDescription { get; set; }

    //    [JsonProperty("share_desc_info")]
    //    public string ShareDetailDescription { get; set; }

    //    [JsonProperty("share_title")]
    //    public string ShareTitle { get; set; }

    //    [JsonProperty("share_title_myself")]
    //    public string ShareTitleSelf { get; set; }

    //    [JsonProperty("share_title_other")]
    //    public string ShareTitleOthers { get; set; }

    //    [JsonProperty("share_url")]
    //    public string ShareUrl { get; set; }

    //    [JsonProperty("share_weibo_desc")]
    //    public string WeiboShareDescription { get; set; }
    //}

    /// <summary>
    /// 抖音短剧统计信息
    /// </summary>
    public class DouyinDramaStats
    {
        [JsonProperty("collect_vv")]
        public int CollectViewCount { get; set; }

        [JsonProperty("current_episode")]
        public int CurrentEpisode { get; set; }

        [JsonProperty("last_added_item_time")]
        public long LastEpisodeAddTimestamp { get; set; }

        [JsonProperty("play_vv")]
        public long TotalPlayViewCount { get; set; }

        [JsonProperty("total_episode")]
        public int TotalEpisodeCount { get; set; }

        [JsonProperty("updated_to_episode")]
        public int LatestEpisodeCount { get; set; }
    }

    /// <summary>
    /// 抖音短剧状态信息
    /// </summary>
    //public class DouyinDramaStatus
    //{
    //    [JsonProperty("is_collected")]
    //    public int IsCollected { get; set; }

    //    [JsonProperty("status")]
    //    public int DramaStatusCode { get; set; }

    //    [JsonProperty("status_desc")]
    //    public string DramaStatusDescription { get; set; }
    //}
    #endregion
}
