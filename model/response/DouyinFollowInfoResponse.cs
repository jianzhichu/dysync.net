using Newtonsoft.Json;

namespace dy.net.model.response
{
    /// <summary>
    /// 关注列表
    /// </summary>
    public class DouyinFollowInfoResponse
    {
        /// <summary>
        /// 
        /// </summary>
        //public Extra extra { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// 
        [JsonProperty("followings")]
        public List<FollowingsItem> Followings { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("has_more")]
        public bool HasMore { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //[JsonProperty("hotsoon_has_more")]
        //public int hotsoon_has_more { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public string hotsoon_text { get; set; }
        ///// <summary>
        /// 
        /// </summary>
        //public Log_pb log_pb { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public int max_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public int min_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public int mix_count { get; set; }
        /// <summary>
        /// 当前登陆人的用户ID
        /// </summary>
        [JsonProperty("myself_user_id")]
        public string MySelfUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public string rec_has_more { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public string store_page { get; set; }
        /// <summary>
        /// 总关注人数
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public int vcd_count { get; set; }
    }



    public class FollowingsItem
    {
        /// <summary>
        /// 头像
        /// </summary>
        [JsonProperty("avatar_larger")]
        public AvatarLarger Avatar { get; set; }

        /// <summary>
        /// 官方账号(企业认证)
        /// </summary>
        [JsonProperty("enterprise_verify_reason")]
        public string EnterpriseVerifyReason { get; set; }

        /// <summary>
        /// 伊朗驻华大使馆
        /// </summary>
        [JsonProperty("nickname")]
        public string NickName { get; set; }

        /// <summary>
        /// 关键--唯一标识符
        /// </summary>
        [JsonProperty("sec_uid")]
        public string SecUid { get; set; }

        ///// <summary>
        ///// 
        ///// </summary>
        //[JsonProperty("short_id")]
        //public string ShortId { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("uid")]
        public string UperId { get; set; }
    }

    /// <summary>
    /// 头像
    /// </summary>
    public class AvatarLarger
    {
        /// <summary>
        /// 
        /// </summary>
        //public int height { get; set; }
        ///// <summary>
        ///// 
        ///// </summary>
        //public string uri { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("url_list")]
        public List<string> UrlList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public int width { get; set; }
    }

    //public class Extra
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<string> fatal_item_ids { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string logid { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int now { get; set; }
    //}

    //public class Avatar_168x168
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int height { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string uri { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<string> url_list { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int width { get; set; }
    //}

    //public class Avatar_300x300
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int height { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string uri { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<string> url_list { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int width { get; set; }
    //}



    //public class Avatar_thumb
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int height { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string uri { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<string> url_list { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int width { get; set; }
    //}

    //public class Aweme_control
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string can_comment { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string can_forward { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string can_share { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string can_show_comment { get; set; }
    //}

    //public class Cover_urlItem
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int height { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string uri { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<string> url_list { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int width { get; set; }
    //}

    //public class Following_list_secondary_information_struct
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int secondary_information_priority { get; set; }
    //    /// <summary>
    //    /// 6个作品未看
    //    /// </summary>
    //    public string secondary_information_text { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int secondary_information_text_type { get; set; }
    //}

    //public class Original_musician
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int digg_count { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int music_count { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int music_used_count { get; set; }
    //}

    //public class Search_impr
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string entity_id { get; set; }
    //}

    //public class Share_qrcode_url
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int height { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string uri { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<string> url_list { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int width { get; set; }
    //}

    //public class Share_info
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string share_desc { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string share_desc_info { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public Share_qrcode_url share_qrcode_url { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string share_title { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string share_title_myself { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string share_title_other { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string share_url { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string share_weibo_desc { get; set; }
    //}

    //public class Urge_detail
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int user_urged { get; set; }
    //}

    //public class Video_icon
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int height { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string uri { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public List<string> url_list { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public int width { get; set; }
    //}





    //public class FollowingsItemxxx
    //{
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string accept_private_policy { get; set; }
    //    ///// <summary>
    //    ///// {"label_style":5,"label_text":"伊朗驻华大使馆官方账号","is_biz_account":1}
    //    ///// </summary>
    //    //public string account_cert_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string account_region { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string account_type { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string activity { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string activity_label { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string ad_cover_title { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string ad_cover_url { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string ad_order_id { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string age_gate_action { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string age_gate_post_action { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string age_gate_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string allow_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string anchor_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string anchor_schedule_guide_txt { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int apple_account { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int authority_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Avatar_168x168 avatar_168x168 { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Avatar_300x300 avatar_300x300 { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string avatar_decoration { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string avatar_decoration_id { get; set; }

    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Avatar_medium avatar_medium { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string avatar_pendant_larger { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string avatar_pendant_medium { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string avatar_pendant_thumb { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Avatar_thumb avatar_thumb { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string avatar_update_reminder { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string avatar_uri { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Aweme_control aweme_control { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int aweme_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string aweme_cover { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int aweme_hotsoon_auth { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string aweme_hotsoon_auth_relation { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public List<string> ban_user_functions { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string bio_email { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string bio_location { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string bio_permission { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string bio_phone { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string bio_secure_url { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string bio_url { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string birthday_hide_level { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string biz_account_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string brand_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string can_modify_hometown_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string can_modify_school_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string can_set_geofencing { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string can_show_group_card { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string cancel_type { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string card_entries { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string card_entries_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string card_entries_not_display { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string card_sort_priority { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string category { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string cha_list { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string clean_following_reason { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string collect_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int comment_filter_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int comment_setting { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string commerce_bubble { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string commerce_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string commerce_permissions { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string commerce_user_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int commerce_user_level { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int constellation { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string contact_name { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string content_language_already_popup { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string count_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string cover_colour { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string cover_jump_url { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public List<Cover_urlItem> cover_url { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int create_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string creator_level { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string custom_verify { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string cv_level { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string display_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string display_wvalantine_activity_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string dog_card_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string dongtai_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string dormer_group { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string dou_plus_share_location { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string douplus_old_user { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string douplus_toast { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int download_prompt_ts { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int download_setting { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string dp_level { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int duet_setting { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string effect_detail { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string enable_nearby_visible { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string enable_wish { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string enterprise_user_info { get; set; }

    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string ever_over_1k_follower { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string fast_comment_texts { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int favoriting_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int fb_expire_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string follow_as_subscription { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string follow_guide { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int follow_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string follow_verify_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int follower_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int follower_request_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int follower_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int following_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Following_list_secondary_information_struct following_list_secondary_information_struct { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string force_private_account { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string forward_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string friend_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string general_permission { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public List<string> geofencing { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string google_account { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_activity_medal { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_card_edit_page_entrance { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_email { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_facebook_token { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_help_desk_entrance { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_insights { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_orders { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_story { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_subscription { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_twitter_token { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_unread_story { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string has_youtube_token { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string hide_following_follower_list { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string hide_location { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string hide_search { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string hide_shoot_button { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string homepage_bottom_toast { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string hometown { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string hometown_fellowship { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string hometown_visible { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string honor_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string hot_list { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string im_age_stage { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string im_examination_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string im_subscription_publisher { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string infringement_report_remind_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string ins_id { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string interest_tags { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_activity_user { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_ad_fake { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_binded_weibo { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_block { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_blocked { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_discipline_member { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_dou_manager { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_effect_artist { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_email_verified { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_equal_query { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_flowcard_member { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_gov_media_vip { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_life_style { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_minor { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_mirror { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_mix_user { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_not_show { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_phone_binded { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_pro_account { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_series_user { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_star { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_top { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string is_verified { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string iso_country_code { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string item_list { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int ky_only_predict { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string language { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string latest_order_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string life_story_block { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int live_agreement { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int live_agreement_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string live_commerce { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int live_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int live_verify { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string login_platform { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string message_chat_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string minor_mode { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string mplatform_followers_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string music_compliance_account { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string name_field { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string need_addr_card { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string need_points { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int need_recommend { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int neiguang_shield { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int new_friend_type { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string new_story_cover { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string new_visitor_count { get; set; }

    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string nickname_update_reminder { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string normal_top_comment_permission { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public List<string> not_seen_item_id_list_v2 { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string notify_private_account { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string open_insight_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Original_musician original_musician { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string personalized_tag { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string platform_sync_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string play_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string post_default_download_setting { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string pr_exempt { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string prevent_download { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string private_account_review_reminder { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string private_aweme_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string pro_account_tcm_red_dot { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string profile_completion { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string profile_pv { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string profile_story { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string profile_tab_type { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string publish_landing_tab { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string punish_remind_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string quick_shop_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string r_fans_group_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int react_setting { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string realname_verify_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string rec_age_stage { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string recommend_reason { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string recommend_reason_relation { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string recommend_score { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string recommend_template { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string recommend_user_reason_source { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int reflow_page_gid { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int reflow_page_uid { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string register_from { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string register_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string relation_label { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string relation_ship { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string relative_users { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string remark_name { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string room_cover { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string room_data { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int room_id { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string room_id_str { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string room_type_tag { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string school_auth { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int school_category { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string school_id { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string school_visible { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Search_impr search_impr { get; set; }

    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int secret { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Share_info share_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string share_qrcode_uri { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int shield_comment_notice { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int shield_digg_notice { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int shield_follow_notice { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string shop_micro_app { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string short_id { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_artist_playlist { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_avatar_decoration_entrance { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_effect_list { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_favorite_list { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_favorite_list_on_item { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_first_avatar_decoration { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_following_follower_banner { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int show_gender_strategy { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_image_bubble { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_located_banner { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_musician_card { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_nearby_active { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_privacy_banner { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_private_tab { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_relation_banner { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_secret_banner { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_subscription { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_tel_book_banner { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string show_user_ban_dialog { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string signature { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int signature_display_lines { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string signature_language { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int special_lock { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string special_state_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string sprint_support_user_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string star_activity_entrance { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string star_billboard_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string star_billboard_rank { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string star_use_new_download { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int stitch_setting { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int story_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string story_expired_guide { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string story_open { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string @string { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int sync_to_toutiao { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string tab_settings { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string third_name { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int total_favorited { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int tw_expire_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string twitter_id { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string twitter_name { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string type_label { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string uid { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string unique_id { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int unique_id_modify_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string unique_id_update_reminder { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Urge_detail urge_detail { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string user_canceled { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string user_deleted { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int user_mode { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int user_not_see { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int user_not_show { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int user_period { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int user_rate { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string user_rate_remind_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string user_rip_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string user_story_count { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string user_tags { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string vcd_auth_block { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string verification_badge_type { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int verification_type { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string verify_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string versatile_display { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string video_cover { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public Video_icon video_icon { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string video_icon_virtual_URI { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string video_unread_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string vs_personal { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string vxe_tag { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string watch_status { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string weibo_name { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string weibo_schema { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string weibo_url { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string weibo_verify { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string white_cover_url { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_commerce_enterprise_tab_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_commerce_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_commerce_newbie_task { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_dou_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_douplus_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_ecp_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_fusion_shop_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_item_commerce_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_luban_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_new_goods { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_shop_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_star_atlas_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_stick_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string with_visitor_shop_entry { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string wx_info { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string wx_tag { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string youtube_channel_id { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string youtube_channel_title { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public int youtube_expire_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string youtube_last_refresh_time { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string youtube_refresh_token { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string yt_raw_token { get; set; }
    //    ///// <summary>
    //    ///// 
    //    ///// </summary>
    //    //public string zero_post_user_task { get; set; }
    //}

    //public class Log_pb
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string impr_id { get; set; }
    //}





    //public class Avatar_medium
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    //public int height { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    //public string uri { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    //public List<string> url_list { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    //public int width { get; set; }
    //}

}
