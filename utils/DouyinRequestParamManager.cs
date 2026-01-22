using dy.net.model.dto;
using System.Collections.Generic;

namespace dy.net.utils
{
    /// <summary>
    /// 抖音网页端请求参数字典管理类
    /// 统一管理各类接口的标准化参数字典，提供全局常量和静态只读参数属性
    /// </summary>
    public static class DouyinRequestParamManager
    {


        #region 基础参数（直接返回模板引用，存在交叉修改风险）
        /// <summary>
        /// 私有静态只读基础参数模板（仅创建1次）
        /// </summary>
        private static readonly Dictionary<string, string> _baseParamTemplate = new Dictionary<string, string>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> InitBaseParams()
        {
            return new Dictionary<string, string>(_baseParamTemplate);
        }
        #endregion

        #region 全局常量（接口调用公共配置）
        /// <summary>
        /// 抖音全局域名
        /// </summary>
        public static readonly string DouyinHost = "https://www.douyin.com";

        /// <summary>
        /// 数据请求客户端标识
        /// </summary>
        public const string DY_HTTP_CLIENT = "douyin-client";

        /// <summary>
        /// 下载客户端标识
        /// </summary>
        public const string DY_HTTP_CLIENT_DOWN = "douyin-client_down";

        /// <summary>
        /// 请求头User-Agent标识
        /// </summary>
        public const string DY_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
        #endregion

        #region 静态只读参数属性（对外暴露的标准化参数字典）
        /// <summary>
        /// 收藏列表参数（用户收藏的内容）
        /// </summary>
        public static Dictionary<string, string> DouyinCollectParams { get; } = InitUserCollectParams();

        /// <summary>
        /// 用户喜欢参数（用户点赞的内容）
        /// </summary>
        public static Dictionary<string, string> DouyinFavoriteParams { get; } = InitUserFavoriteParams();

        /// <summary>
        /// 博主发布作品参数
        /// </summary>
        public static Dictionary<string, string> DouyinUpderPostParams { get; } = InitDouyinPostParams();

        /// <summary>
        /// 我的关注列表参数
        /// </summary>
        public static Dictionary<string, string> DouyinMyFollowParams { get; } = InitMyFollowParams();

        /// <summary>
        /// 收藏夹按收藏文件夹名称获取作品列表完整请求参数字典
        /// </summary>
        public static Dictionary<string, string> DouyinFolderCollectParams { get; } = InitCollectFolderParams();

        /// <summary>
        /// 抖音收藏列表接口完整参数字典（对应目标URL）
        /// </summary>
        public static Dictionary<string, string> DouyinCollectListParams { get; } = InitCollectListParams();

        /// <summary>
        /// 抖音合集列表参数
        /// </summary>
        public static Dictionary<string, string> DouyinMixListParams { get; } = InitMixListParams();

        /// <summary>
        /// 合集视频请求参数字典
        /// </summary>
        public static Dictionary<string, string> DouyinMixVideoParams { get; } = InitMixVideoParams();

        /// <summary>
        /// 抖音短剧列表参数
        /// </summary>
        public static Dictionary<string, string> DouyinSeriesListParams { get; } = InitSeriesListParams();

        /// <summary>
        /// 抖音短剧视频参数（修正拼写：Viedos→Videos）
        /// </summary>
        public static Dictionary<string, string> DouyinSeriesVideosParams { get; } = InitSeriesVideosParams();
        #endregion

        #region 私有初始化方法（短剧相关）
        /// <summary>
        /// 初始化抖音短剧视频参数（获取短剧内的视频列表）
        /// </summary>
        private static Dictionary<string, string> InitSeriesVideosParams()
        {
            var parameters = InitBaseParams();

            // 覆盖基础参数（直接赋值，仅修改与基础参数不一致的项）
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
            parameters["round_trip_time"] = "50";
            parameters["webid"] = "7584476599598188067";

            // 新增特有参数（使用Add方法，明确区分"覆盖"与"新增"，提高可读性）
            parameters.Add("pull_type", "2");

            parameters.Add("series_id", ""); // 动态参数：短剧系列唯一标识，后续赋值
            parameters.Add("cursor", "0");   // 动态参数：分页游标，默认从0开始
            parameters.Add("count", "20");   // 动态参数：每页获取数量，默认20条

            return parameters;
        }

        /// <summary>
        /// 初始化抖音短剧列表参数
        /// </summary>
        private static Dictionary<string, string> InitSeriesListParams()
        {
            var parameters = InitBaseParams();

            // 直接赋值覆盖基础参数
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
            parameters["round_trip_time"] = "50";
            parameters["webid"] = "7584476599598188067";

            // 新增特有分页参数
            parameters.Add("cursor", "0");
            parameters.Add("count", "20");

            return parameters;
        }
        #endregion

        #region 私有初始化方法（合集相关）
        /// <summary>
        /// 初始化抖音合集列表请求参数
        /// </summary>
        private static Dictionary<string, string> InitMixListParams()
        {
            var parameters = InitBaseParams();

            // 直接赋值覆盖基础参数（严格对应目标接口）
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
            parameters["webid"] = "7584476599598188067";

            // 新增特有分页参数
            parameters.Add("cursor", "0");
            parameters.Add("count", "20");

            return parameters;
        }

        /// <summary>
        /// 初始化合集视频请求参数（复用基础参数，消除冗余）
        /// </summary>
        private static Dictionary<string, string> InitMixVideoParams()
        {
            var parameters = InitBaseParams();

            // 覆盖基础参数，无需重复定义全部公共参数
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
            parameters["webid"] = "7584476599598188067";

            // 新增特有参数
            parameters.Add("mix_id", "");    // 动态参数：合集唯一标识，后续赋值
            parameters.Add("cursor", "0");   // 动态参数：分页游标，默认从0开始
            parameters.Add("count", "20");   // 动态参数：每页获取数量，默认20条

            return parameters;
        }
        #endregion

        #region 私有初始化方法（收藏相关）
        /// <summary>
        /// 初始化抖音收藏列表接口参数
        /// </summary>
        private static Dictionary<string, string> InitCollectListParams()
        {
            var parameters = InitBaseParams();

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
            parameters["webid"] = "7584476599598188067";

            // 新增特有动态参数
            parameters.Add("cursor", "0");  // 默认分页游标为0，后续可修改
            parameters.Add("count", "");   // 动态参数：每页获取数量，后续赋值

            return parameters;
        }

        /// <summary>
        /// 初始化收藏夹按文件夹查询作品参数
        /// </summary>
        private static Dictionary<string, string> InitCollectFolderParams()
        {
            var parameters = InitBaseParams();

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
            parameters["webid"] = "7584476599598188067";

            // 新增特有动态参数
            parameters.Add("collects_id", ""); // 动态参数：收藏夹ID，后续赋值 collect.CollectsId
            parameters.Add("cursor", "");      // 动态参数：分页游标，后续赋值
            parameters.Add("count", "");       // 动态参数：每页获取数量，后续赋值

            return parameters;
        }
        #endregion

        #region 私有初始化方法（作品/收藏/关注相关）
        /// <summary>
        /// 初始化抖音博主发布作品参数（修正访问修饰符：public→private）
        /// </summary>
        private static Dictionary<string, string> InitDouyinPostParams()
        {
            var parameters = InitBaseParams();

            // 覆盖基础参数
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
            parameters["webid"] = "7574080345697584675";

            // 新增特有参数（含签名相关空参数，后续补充）
            parameters.Add("locate_item_id", "7576282367263807451");
            parameters.Add("locate_query", "false");
            parameters.Add("show_live_replay_strategy", "1");
            parameters.Add("need_time_list", "1");
            parameters.Add("time_list_query", "0");
            parameters.Add("publish_video_strategy_type", "2");
            parameters.Add("from_user_page", "1");
            parameters.Add("uifid", "");
            parameters.Add("msToken", "");
            parameters.Add("a_bogus", "");
            parameters.Add("verifyFp", "");
            parameters.Add("fp", "");
            parameters.Add("x-secsdk-web-expire", "");
            parameters.Add("x-secsdk-web-signature", "");
            parameters.Add("cut_version", "1");



            parameters.Add("sec_user_id", ""); // 动态参数：博主加密ID，后续赋值
            parameters.Add("max_cursor", "0"); // 分页游标，默认从0开始
            parameters.Add("count", "18");     // 每页默认获取18条作品

            return parameters;
        }

        /// <summary>
        /// 初始化默认的收藏列表参数
        /// </summary>
        private static Dictionary<string, string> InitUserCollectParams()
        {
            var parameters = InitBaseParams();

            // 覆盖基础参数
            parameters["version_code"] = "290100";
            parameters["version_name"] = "29.1.0";
            parameters["screen_width"] = "1920";
            parameters["screen_height"] = "1080";
            parameters["browser_version"] = "130.0.0.0";
            parameters["engine_version"] = "130.0.0.0";
            parameters["cpu_core_num"] = "12";

            parameters["from_user_page"] = "1";
            parameters["locate_query"] = "false";
            parameters["need_time_list"] = "1";
            parameters["show_live_replay_strategy"] = "1";
            parameters["time_list_query"] = "0";


            return parameters;
        }

        /// <summary>
        /// 初始化用户喜欢（点赞）列表参数
        /// </summary>
        private static Dictionary<string, string> InitUserFavoriteParams()
        {
            var parameters = InitBaseParams();

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

            // 新增特有参数
            parameters.Add("min_cursor", "0");
            parameters.Add("cut_version", "1");
            parameters.Add("count", "18");

            return parameters;
        }

        /// <summary>
        /// 初始化我关注的博主列表参数
        /// </summary>
        private static Dictionary<string, string> InitMyFollowParams()
        {
            var parameters = InitBaseParams();

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
            parameters["webid"] = "7577203855940994560";

            // 新增特有参数（含动态用户ID和签名参数）
            parameters.Add("min_time", "0");
            parameters.Add("max_time", "0");
            parameters.Add("source_type", "4");
            parameters.Add("gps_access", "0");
            parameters.Add("address_book_access", "0");
            parameters.Add("is_top", "1");
            //parameters.Add("uifid", "");
            //parameters.Add("msToken", "");
            //parameters.Add("a_bogus", "");
            //parameters.Add("verifyFp", "");
            //parameters.Add("fp", "");


            parameters.Add("user_id", "");      // 动态参数：用户ID，后续赋值
            parameters.Add("sec_user_id", "");  // 动态参数：加密用户ID，后续赋值
            parameters.Add("offset", "0");      // 分页偏移量，默认从0开始
            parameters.Add("count", "20");      // 每页默认获取20个关注博主

            return parameters;
        }
        #endregion

        /// <summary>
        /// 获取视频类型名称
        /// </summary>
        /// <param name="videoType"></param>
        /// <returns></returns>
        public static string GetDesc(this VideoTypeEnum videoType)
        {
            return videoType switch
            {
                VideoTypeEnum.dy_favorite => "喜欢",
                VideoTypeEnum.dy_collects => "默认收藏夹",
                VideoTypeEnum.dy_follows => "关注",
                VideoTypeEnum.ImageVideo => "图文视频",
                VideoTypeEnum.dy_custom_collect => "自定义收藏夹",
                VideoTypeEnum.dy_mix => "合集",
                VideoTypeEnum.dy_series => "短剧",
                _ => string.Empty // 匹配所有未定义的枚举值，返回空字符串（替代原 default）
            };
        }
    }
}