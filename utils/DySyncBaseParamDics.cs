namespace dy.net.utils
{
    public class DySyncBaseParamDics
    {
        public static Dictionary<string, string> CollectParams { get; } = InitializeUserCollecParams();
        public static Dictionary<string, string> FavoriteParams { get; } = InitializeUserFavoriteParams();
        public static Dictionary<string, string> UpderPostParams { get; } = InitializeUpderPostParams();

        private static Dictionary<string, string> InitializeUserCollecParams()
        {
            var parameters = GetDyBaseParameters();

            // 覆盖与基础字段不同的值
            parameters["version_code"] = "290100";
            parameters["version_name"] = "29.1.0";
            parameters["screen_width"] = "1920";
            parameters["screen_height"] = "1080";
            parameters["browser_version"] = "130.0.0.0";
            parameters["engine_version"] = "130.0.0.0";
            parameters["cpu_core_num"] = "12";

            // 添加当前字段特有的键值对
            parameters["from_user_page"] = "1";
            parameters["locate_query"] = "false";
            parameters["need_time_list"] = "1";
            parameters["show_live_replay_strategy"] = "1";
            parameters["time_list_query"] = "0";

            return parameters;
        }

        // 初始化用户收藏参数
        private static Dictionary<string, string> InitializeUserFavoriteParams()
        {
            var parameters = GetDyBaseParameters();

            // 覆盖与基础字段不同的值
            parameters["version_code"] = "170400";
            parameters["version_name"] = "17.4.0";
            parameters["screen_width"] = "1536";
            parameters["screen_height"] = "960";
            parameters["browser_version"] = "140.0.0.0";
            parameters["engine_version"] = "140.0.0.0";
            parameters["cpu_core_num"] = "20";

            // 添加当前字段特有的键值对
            parameters["min_cursor"] = "0";
            parameters["cut_version"] = "1";
            parameters["count"] = "18";
            parameters["support_h265"] = "1";
            parameters["support_dash"] = "1";

            return parameters;
        }

        // 初始化抖音博主发布作品参数
        private static Dictionary<string,string> InitializeUpderPostParams()
        {
            var parameters = new Dictionary<string, string>
        {
            { "WebIdLastTime", "1714385892" },
            { "aid", "1988" },
            { "app_language", "zh-Hans" },
            { "app_name", "tiktok_web" },
            { "browser_language", "zh-CN" },
            { "browser_name", "Mozilla" },
            { "browser_online", "true" },
            { "browser_platform", "Win32" },
            { "browser_version", "5.0%20%28Windows%29" },
            { "channel", "tiktok_web" },
            { "cookie_enabled", "true" },
            { "count", "18" },
            { "coverFormat", "2" },
            { "cursor", "0" },
            { "data_collection_enabled", "true" },
            { "device_id", "7380187414842836523" },
            { "device_platform", "web_pc" },
            { "focus_state", "true" },
            { "from_page", "user" },
            { "history_len", "3" },
            { "is_fullscreen", "false" },
            { "is_page_visible", "true" },
            { "language", "zh-Hans" },
            { "locate_item_id", "" },
            { "needPinnedItemIds", "true" },
            { "odinId", "7404669909585003563" },
            { "os", "windows" },
            { "post_item_list_request_type", "0" },
            { "priority_region", "US" },
            { "referer", "" },
            { "region", "US" },
            { "screen_height", "827" },
            { "screen_width", "1323" },
            { "secUid", "" },
            { "tz_name", "America%2FLos_Angeles" },
            { "user_is_login", "true" },
            { "webcast_language", "zh-Hans" },
            { "msToken", "" },
            { "_signature", "_02B4Z6wo000017oyWOQAAIDD9xNhTSnfaDu6MFxAAIlj23" },
            {"sec_user_id",""}
        };
            return parameters;
        }

        // 静态基础参数（供内部初始化使用）
        private static Dictionary<string, string> GetDyBaseParameters()
        {
            return new Dictionary<string, string>
            {
                {"device_platform", "webapp"},
                {"aid", "6383"},
                {"channel", "channel_pc_web"},
                {"pc_client_type", "1"},
                {"cookie_enabled", "true"},
                {"browser_language", "zh-CN"},
                {"browser_platform", "Win32"},
                {"browser_name", "Chrome"},
                {"browser_online", "true"},
                {"engine_name", "Blink"},
                {"os_name", "Windows"},
                {"os_version", "10"},
                {"device_memory", "8"},
                {"platform", "PC"},
                {"downlink", "10"},
                {"effective_type", "4g"},
                {"pc_libra_divert", "Windows"},
                {"publish_video_strategy_type", "2"},
                {"round_trip_time", "0"},
                {"whale_cut_token", ""},
                {"update_version_code", "170400"}
            };
        }

    }
}