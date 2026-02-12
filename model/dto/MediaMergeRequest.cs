namespace dy.net.model.dto
{
    public class MediaMergeRequest
    {
        /// <summary>网络图片地址数组（必填）</summary>
        public List<DouyinMergeVideoDto> ImageUrls { get; set; }

        /// <summary>网络MP3地址数组（必填）</summary>
        public List<string> AudioUrls { get; set; }

        /// <summary>每张图片显示时长（秒，默认3秒）</summary>
        public int ImageDurationPerSecond { get; set; } = 3;

        /// <summary>视频分辨率（格式：1920x1080，默认1920x1080）</summary>
        public int VideoWidth { get; set; } = 1080;
        public int VideoHeight { get; set; } = 1920;

        /// <summary>输出视频格式（默认mp4）</summary>
        public string OutputFormat { get; set; } = "mp4";

        /// <summary>视频帧率（默认25）</summary>
        public int VideoFps { get; set; } = 25;
    }
}
