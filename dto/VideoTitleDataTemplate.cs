namespace dy.net.dto
{
    public class VideoTitleDataTemplate
    {
        /// <summary> 对应 {id} </summary>
        public string Id { get; set; }

        /// <summary> 对应 {VideoTitle} </summary>
        public string VideoTitle { get; set; } = string.Empty;

        /// <summary> 对应 {SyncTime}（同步时间） </summary>
        //public DateTime? SyncTime { get; set; }

        /// <summary> 对应 {ReleaseTime}（发布时间） </summary>
        public DateTime? ReleaseTime { get; set; }

        /// <summary> 对应 {FileHash}（文件哈希值） </summary>
        public string FileHash { get; set; } = string.Empty;

        /// <summary> 对应 {Resolution}（分辨率，如 1920x1080） </summary>
        public string Resolution { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        /// <summary> 对应 {FileSize}（文件大小，单位：字节） </summary>
        //public long FileSize { get; set; } = 0;
    }
}
