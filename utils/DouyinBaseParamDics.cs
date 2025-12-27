using System.Collections.Generic;

namespace dy.net.utils
{
    /// <summary>
    /// 抖音网页端请求参数字典管理类
    /// 提供各类接口的标准化参数字典，对外暴露的属性名称和返回类型保持不变
    /// </summary>
    public static class DouyinBaseParamDics
    {
        /// <summary>
        /// 收藏列表参数（用户收藏的内容）
        /// </summary>
        public static Dictionary<string, string> CollectParams { get; } = InitializeUserCollectParams();

        /// <summary>
        /// 用户收藏参数（与 CollectParams 功能区分，保留原始定义）
        /// </summary>
        public static Dictionary<string, string> FavoriteParams { get; } = InitializeUserFavoriteParams();

        /// <summary>
        /// 博主发布作品参数
        /// </summary>
        public static Dictionary<string, string> UpderPostParams { get; } = InitializeUpderPostParams();

        /// <summary>
        /// 我关注的博主列表参数
        /// </summary>
        public static Dictionary<string, string> MyFollowParams { get; } = InitializeMyFollowParams();

        /// <summary>
        /// 视频详情
        /// </summary>
        public static Dictionary<string, string> ViedoDetailParam { get; } = InitializeViedoDetailParam();

        /// <summary>
        /// 抖音作品列表请求参数（适配接口：https://www.douyin.com/aweme/v1/web/aweme/post/）
        /// </summary>
        public static Dictionary<string, string> InitializeDouyinPostParams()
        {
            // 基于基础参数扩展，减少冗余
            var parameters = GetBaseParameters();

            // 覆盖基础参数中不同的值
            parameters["version_code"] = "290100";
            parameters["version_name"] = "29.1.0";
            parameters["screen_width"] = "1707";
            parameters["screen_height"] = "1067";
            parameters["browser_name"] = "Chrome";
            parameters["browser_version"] = "142.0.0.0";
            parameters["engine_version"] = "142.0.0.0";
            parameters["cpu_core_num"] = "32";
            parameters["support_h265"] = "1";
            parameters["support_dash"] = "1";

            // 添加特有参数
            parameters.AddRange(new Dictionary<string, string>
            {
                {"sec_user_id", ""},
                {"max_cursor", "0"},
                {"locate_item_id", "7576282367263807451"},
                {"locate_query", "false"},
                {"count", "18"},
                {"show_live_replay_strategy", "1"},
                {"need_time_list", "1"},
                {"time_list_query", "0"},
                {"publish_video_strategy_type", "2"},
                {"from_user_page", "1"},
                {"webid", "7574080345697584675"},
                {"uifid", ""},
                {"msToken", ""},
                {"a_bogus", ""},
                {"verifyFp", ""},
                {"fp", ""},
                {"x-secsdk-web-expire", ""},
                {"x-secsdk-web-signature", ""},
                {"cut_version", "1"}
            });

            return parameters;
        }

        #region 私有初始化方法

        /// <summary>
        /// 初始化收藏列表参数
        /// </summary>
        private static Dictionary<string, string> InitializeUserCollectParams()
        {
            var parameters = GetBaseParameters();

            // 覆盖基础参数
            parameters["version_code"] = "290100";
            parameters["version_name"] = "29.1.0";
            parameters["screen_width"] = "1920";
            parameters["screen_height"] = "1080";
            parameters["browser_version"] = "130.0.0.0";
            parameters["engine_version"] = "130.0.0.0";
            parameters["cpu_core_num"] = "12";

            // 添加特有参数
            parameters.AddRange(new Dictionary<string, string>
            {
                {"from_user_page", "1"},
                {"locate_query", "false"},
                {"need_time_list", "1"},
                {"show_live_replay_strategy", "1"},
                {"time_list_query", "0"}
            });

            return parameters;
        }

        /// <summary>
        /// 初始化用户收藏参数
        /// </summary>
        private static Dictionary<string, string> InitializeUserFavoriteParams()
        {
            var parameters = GetBaseParameters();

            // 覆盖基础参数
            parameters["version_code"] = "170400";
            parameters["version_name"] = "17.4.0";
            parameters["screen_width"] = "1536";
            parameters["screen_height"] = "960";
            parameters["browser_version"] = "140.0.0.0";
            parameters["engine_version"] = "140.0.0.0";
            parameters["cpu_core_num"] = "20";
            parameters["support_h265"] = "1";
            parameters["support_dash"] = "1";

            // 添加特有参数
            parameters.AddRange(new Dictionary<string, string>
            {
                {"min_cursor", "0"},
                {"cut_version", "1"},
                {"count", "18"}
            });

            return parameters;
        }

        /// <summary>
        /// 初始化博主发布作品参数
        /// </summary>
        private static Dictionary<string, string> InitializeUpderPostParams()
        {
            // 该接口参数差异较大，单独初始化（保留原始参数完整）
            return new Dictionary<string, string>
            {
                {"WebIdLastTime", "1714385892"},
                {"aid", "1988"},
                {"app_language", "zh-Hans"},
                {"app_name", "tiktok_web"},
                {"browser_language", "zh-CN"},
                {"browser_name", "Mozilla"},
                {"browser_online", "true"},
                {"browser_platform", "Win32"},
                {"browser_version", "5.0%20%28Windows%29"},
                {"cookie_enabled", "true"},
                {"count", "18"},
                {"coverFormat", "2"},
                {"cursor", "0"},
                {"data_collection_enabled", "true"},
                {"device_id", "7380187414842836523"},
                {"device_platform", "webapp"},
                {"channel", "channel_pc_web"},
                {"focus_state", "true"},
                {"from_page", "user"},
                {"history_len", "3"},
                {"is_fullscreen", "false"},
                {"is_page_visible", "true"},
                {"language", "zh-Hans"},
                {"locate_item_id", ""},
                {"needPinnedItemIds", "true"},
                {"odinId", "7404669909585003563"},
                {"os", "windows"},
                {"post_item_list_request_type", "0"},
                {"priority_region", "US"},
                {"referer", ""},
                {"region", "US"},
                {"screen_height", "827"},
                {"screen_width", "1323"},
                {"secUid", ""},
                {"sec_user_id", ""},
                {"tz_name", "America%2FLos_Angeles"},
                {"user_is_login", "true"},
                {"webcast_language", "zh-Hans"},
                {"msToken", ""}
            };
        }

        /// <summary>
        /// 初始化我关注的博主列表参数
        /// </summary>
        private static Dictionary<string, string> InitializeMyFollowParams()
        {
            var parameters = GetBaseParameters();

            // 覆盖基础参数
            parameters["version_code"] = "170400";
            parameters["version_name"] = "17.4.0";
            parameters["screen_width"] = "1707";
            parameters["screen_height"] = "1067";
            parameters["browser_name"] = "Edge";
            parameters["browser_version"] = "141.0.0.0";
            parameters["engine_version"] = "141.0.0.0";
            parameters["cpu_core_num"] = "32";
            parameters["support_h265"] = "1";
            parameters["support_dash"] = "1";

            // 添加特有参数
            parameters.AddRange(new Dictionary<string, string>
            {
                {"user_id", ""},//空着
                {"sec_user_id", ""},
                {"offset", "40"},
                {"min_time", "0"},
                {"max_time", "0"},
                {"count", "20"},
                {"source_type", "4"},
                {"gps_access", "0"},
                {"address_book_access", "0"},
                {"is_top", "1"},
                {"webid", "7577203855940994560"},
                {"uifid", ""},
                {"msToken", ""},
                {"a_bogus", ""},
                {"verifyFp", ""},
                {"fp", ""}
            });

            return parameters;
        }

        #endregion

        #region 基础参数与扩展方法

        /// <summary>
        /// 获取抖音网页端基础参数字典（所有接口的公共参数）
        /// </summary>
        private static Dictionary<string, string> GetBaseParameters()
        {
            return new Dictionary<string, string>
            {
                {"device_platform", "webapp"},
                {"aid", "6383"},
                {"channel", "channel_pc_web"},
                {"pc_client_type", "1"},
                {"pc_libra_divert", "Windows"},
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
                {"round_trip_time", "0"},
                {"update_version_code", "170400"},
                {"whale_cut_token", ""}
            };
        }


        private static Dictionary<string, string> InitializeViedoDetailParam()
        {

            Dictionary<string, string> requestParams = new Dictionary<string, string>
{
                { "device_platform", "webapp" },
                { "aid", "6383" },
                { "channel", "channel_pc_web" },
                { "aweme_id", "" },
                { "request_source", "600" },
                { "origin_type", "video_page" },
                { "update_version_code", "170400" },
                { "pc_client_type", "1" },
                { "pc_libra_divert", "Windows" },
                { "support_h265", "1" },
                { "support_dash", "1" },
                { "cpu_core_num", "32" },
                { "version_code", "190500" },
                { "version_name", "19.5.0" },
                { "cookie_enabled", "true" },
                { "screen_width", "1707" },
                { "screen_height", "1067" },
                { "browser_language", "zh-CN" },
                { "browser_platform", "Win32" },
                { "browser_name", "Edge" },
                { "browser_version", "141.0.0.0" },
                { "browser_online", "true" },
                { "engine_name", "Blink" },
                { "engine_version", "141.0.0.0" },
                { "os_name", "Windows" },
                { "os_version", "10" },
                { "device_memory", "8" },
                { "platform", "PC" },
                { "downlink", "10" },
                { "effective_type", "4g" },
                { "round_trip_time", "0" },
                { "webid", "7577203855940994560" },
    { "uifid", "0e81ba593d64ebaca259bdbe302de8d7e55ac2e982f7412f10fbc5c77c64bb8b8830ffed112ea8b3bc54f3b6e2af3856daafa4efaafef18953612820da493aeaebdeac0537b99b8e68f343c2476cf6347f7751a87d92952128116470f147215cf46b949f43f31aedcb660fd3c5c7eb2145183cc93d1b4202205b4af7d7c69ab55e860f96e315899a2ee74a262273694ec973ba682bb6fdc351e0e66250b21cf9aef3ccaa3e0c28b085fd095e947d92a5336b9e65706d649a7b79541feab3487f" },
    { "verifyFp", "verify_migsr5je_BJ1YiVbY_uR2U_4VXu_8Uje_GgwhVf7Y6hb8" },
    { "fp", "verify_migsr5je_BJ1YiVbY_uR2U_4VXu_8Uje_GgwhVf7Y6hb8" },
    //{ "msToken", "zVKqVOb-uBF-Opse1B5y9u0hw7SlRa49w8HB56cVSY0Uymjqs17JJ9qOUXv7IK0j3de1ErBjSVAm0JSd5SH1sYg7jeriVvGNJoDUM3dKwVCrqlapZgU9g2aYAeHs5oHddTGtZ3Pri9MIRltXVRdirZWMewfH8MlqmIVaCiTuGzRcZ6va9aWe0CyR" },
    //{ "a_bogus", "dy45kFWjOxRfOdFtmOnc9WxlY8L%2FNTuy6Pi2SYAP9PKGcwFcaWNpBNCfrxLuRUd%2FzuBzhe3HqdlMYDnc0zX0ZenkKmkkupv6Bt%2FC9L0LZZHvbBJZ7rgiemSxzk4O8KsOmAIbiM75AsBEIxo5VrCwAdlCu%2F-xBbmD%2Fp3vVATCE2ysUAujwn%2FVa-JDNw7qaf%3D%3D" },
    //{ "x-secsdk-web-expire", "1764378577130" },
    //{ "x-secsdk-web-signature", "3b8aa32e9457ebe3b65afdae11e2fe86" }
};
            return requestParams;
        }


        /// <summary>
        /// 字典扩展方法：批量添加键值对（避免重复 Add 调用）
        /// </summary>
        private static void AddRange(this Dictionary<string, string> target, Dictionary<string, string> source)
        {
            foreach (var item in source)
            {
                // 避免键重复（如果有重复以源字典为准）
                if (target.ContainsKey(item.Key))
                    target[item.Key] = item.Value;
                else
                    target.Add(item.Key, item.Value);
            }
        }

        #endregion
    }
}