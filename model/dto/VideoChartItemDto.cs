namespace dy.net.model.dto
{
    public class VideoChartItemDto
    {
        /// <summary>
        /// 日期，格式MM-DD（如01-21）
        /// </summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>
        /// 我喜欢的数量
        /// </summary>
        public int Favorite { get; set; }

        /// <summary>
        /// 我收藏的数量
        /// </summary>
        public int Collect { get; set; }

        /// <summary>
        /// 我关注的数量
        /// </summary>
        public int Follow { get; set; }

        /// <summary>
        /// 图文视频数量
        /// </summary>
        public int Graphic { get; set; }

        /// <summary>
        /// 合集数量
        /// </summary>
        public int Mix { get; set; }

        /// <summary>
        /// 短剧数量
        /// </summary>
        public int Series { get; set; }
    }
}
