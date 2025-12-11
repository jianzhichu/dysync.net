using System.ComponentModel.DataAnnotations;

namespace dy.net.dto
{
    public class LogFileInfoDto
    {


        /// <summary>
        /// 日志类型（debug/error）
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 日志日期（格式：20251203）
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 完整文件名
        /// </summary>
        public string FileName { get; set; }
    }


    public class LogContentRequest
    {
        /// <summary>
        /// 日志类型
        /// </summary>
        [Required]
        public string Type { get; set; }

        /// <summary>
        /// 日志日期
        /// </summary>
        [Required]
        public string Date { get; set; }
    }
}
