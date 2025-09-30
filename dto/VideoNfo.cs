namespace dy.net.dto
{
    public class VideoNfo
    {

        /// 视频名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 封面图片路径或URL
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// 海报图片路径或URL
        /// </summary>
        public string Poster { get; set; }

        /// <summary>
        /// 作者/创作者
        /// </summary>
        public string Author { get; set; }
    }
}
