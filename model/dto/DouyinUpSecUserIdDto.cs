using Newtonsoft.Json;

namespace dy.net.model.dto
{
    public class DouyinUpSecUserIdDto
    {

        /// <summary>
        /// up主名称
        /// </summary>
        public string uper { get; set; }

        public string uid { get; set; }
        /// <summary>
        /// 是否同步up主全部视频 0-否 1-是
        /// </summary>
        public bool syncAll { get; set; }
    }
}
