using Newtonsoft.Json;

namespace dy.net.dto
{
    public class FollowErrorDto
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("followings")]
        public object Followings { get; set; }

        /// <summary>
        /// 8 - 用户未登录
        /// </summary>
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        /// <summary>
        /// 用户未登录
        /// </summary>
        [JsonProperty("status_msg")]
        public string StatusMsg { get; set; }
    }
}
