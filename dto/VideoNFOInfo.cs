namespace dy.net.dto
{
    public class VideoNFOInfo
    {

        // 基本信息
        /// <summary>
        /// 视频标题（通常是本地化标题）
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 原始标题（通常是影片的原名，如外语片的原名）
        /// </summary>
        public string OriginalTitle { get; set; }

        /// <summary>
        /// 排序标题，用于媒体库排序时使用
        /// </summary>
        public string SortTitle { get; set; }

        /// <summary>
        /// 发行年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 详细剧情简介
        /// </summary>
        public string Plot { get; set; }

        /// <summary>
        /// 剧情大纲（比Plot更简短的描述）
        /// </summary>
        public string Outline { get; set; }

        /// <summary>
        /// 宣传语、标语（影片的简短宣传句子）
        /// </summary>
        public string Tagline { get; set; }

        // 人员信息
        /// <summary>
        /// 导演姓名
        /// </summary>
        public string Director { get; set; }

        /// <summary>
        /// 演员列表
        /// </summary>
        public List<string> Actors { get; set; } = new List<string>();

        /// <summary>
        /// 编剧列表
        /// </summary>
        public List<string> Writers { get; set; } = new List<string>();

        // 媒体信息
        /// <summary>
        /// 类型（如动作、喜剧、科幻等，多个类型用逗号分隔）
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// 评分（通常是10分制）
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// 评分人数
        /// </summary>
        public int Votes { get; set; }

        /// <summary>
        /// 制作公司
        /// </summary>
        public string Studio { get; set; }

        /// <summary>
        /// 首映日期
        /// </summary>
        public DateTime? Premiered { get; set; }

        /// <summary>
        /// 片长（通常以分钟为单位，如"120分钟"）
        /// </summary>
        public string Runtime { get; set; }

        // 文件信息
        /// <summary>
        /// 文件名及路径
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件大小（以字节为单位）
        /// </summary>
        public long FileSize { get; set; }

        // 可根据需要添加更多字段
        /// <summary>
        /// 国家/地区（制作国家或地区）
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 语言（影片使用的语言）
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 视频编码格式（如H.264, H.265等）
        /// </summary>
        public string VideoCodec { get; set; }

        /// <summary>
        /// 音频编码格式（如AC3, DTS等）
        /// </summary>
        public string AudioCodec { get; set; }

        /// <summary>
        /// 分辨率（如1920x1080, 3840x2160等）
        /// </summary>
        public string Resolution { get; set; }
    }
}
