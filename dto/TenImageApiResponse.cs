namespace dy.net.dto
{
    //如果好用，请收藏地址，帮忙分享。
    public class ImageTagInfoItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string Architecture { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreationTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DockerVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DurationDays { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ImageId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Kind { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PushTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int SizeByte { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TagId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdateTime { get; set; }
    }

    public class ImageData
    {
        /// <summary>
        /// 
        /// </summary>
        public string RepoName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int TagCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ImageTagInfoItem> TagInfo { get; set; }
    }

    public class ImageResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public ImageData Data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RequestId { get; set; }
    }

    public class TenImageApiResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public ImageResponse Response { get; set; }
    }

}
