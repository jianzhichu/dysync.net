using ClockSnowFlake;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace dy.net.utils
{
    /// <summary>
    /// 抖音标题转文件名工具类（兼容Windows/macOS/Linux）
    /// </summary>
    public static class DouyinFileNameHelper
    {
        #region 配置参数
        /// <summary>
        /// 建议120字节（约60个汉字），适配所有系统
        /// </summary>
        private const int MaxFileNameBytes = 120;



        /// <summary>
        /// 非法字符替换后的占位符（也可设为空字符串）
        /// </summary>
        private const string IllegalCharReplacement = "";


        // 修正：使用 \U 前缀来表示超过 \uFFFF 的Unicode码点
        private static readonly Regex _emojiRegex = new Regex(
            @"[\u1F600-\u1F64F\u1F300-\u1F5FF\u1F680-\u1F6FF\U0001E000-\U0001EFFF\u2600-\u2B55\u200D]",
            RegexOptions.Compiled);

        private static readonly Regex _hashtagRegex = new Regex(@"\#\S+", RegexOptions.Compiled);
        private static readonly Regex _invalidCharsRegex;
        private static readonly Regex _multipleUnderscoresRegex = new Regex(@"_+", RegexOptions.Compiled);

        #endregion

        #region 处理抖音视频文件名
        /// <summary>
        /// 生成抖音视频文件名
        /// </summary>
        /// <param name="originalTitle">抖音原始标题</param>
        /// <param name="videoId">排序号--如果有重复标题，根据时间排序取最后一个的序号+1 从001开始</param>
        /// <returns>安全可用的文件名</returns>
        public static string GenerateFileName(string originalTitle,string videoId)
        {
            // 1. 基础容错（处理空值）
            originalTitle = string.IsNullOrWhiteSpace(originalTitle) ? videoId : originalTitle;

            // 2. 净化标题：移除非法内容
            string purifiedTitle = PurifyTitle(originalTitle, videoId);

            // 3. 长度控制：按UTF-8字节数截断（避免超系统限制）
            string truncatedTitle = TruncateByByteLength(purifiedTitle, MaxFileNameBytes);

            return truncatedTitle.Trim();
        }
        #endregion

        #region 内部辅助方法：标题净化
        /// <summary>
        /// 净化标题（移除非法字符、表情、话题标签）
        /// </summary>
        private static string PurifyTitle(string title,string id)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = id;
            }


            title = Regex.Replace(title, @"[^a-zA-Z0-9\u4e00-\u9fa5]", "-");
            // 步骤1：移除话题标签（#xxx 或 #xxx#yyy）
            //title = Regex.Replace(title, @"#\S+", "", RegexOptions.Compiled);

            // 步骤2：移除表情符号（匹配常见表情Unicode区块）
            //string emojiPattern = @"[\u1F600-\u1F64F\u1F300-\u1F5FF\u1F680-\u1F6FF\u1E000-\u1EFFF\u2600-\u2B55\u200D]";
            //title = Regex.Replace(title, emojiPattern, "", RegexOptions.Compiled);

            // 步骤3：过滤系统非法字符（Windows/macOS/Linux通用禁止）
            char[] illegalChars = new[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|', '\0', '\t', '\n', '\r' };
            foreach (char c in illegalChars)
            {
                title = title.Replace(c.ToString(), IllegalCharReplacement);
            }
            char Separator = '-';

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                title = title.Replace(c, '_');
            }


            // 步骤5：合并连续分隔符（避免多个---）
            title = Regex.Replace(title, $"{Separator}+", Separator.ToString(), RegexOptions.Compiled);

            // 步骤6：移除首尾无效字符（分隔符、点号）
            title = title.Replace(" ","").Trim(Separator, '.');

            // 容错：如果净化后为空，返回默认值
            return string.IsNullOrWhiteSpace(title) ? id : title;
        }
        #endregion

        #region 内部辅助方法：按字节数截断
        /// <summary>
        /// 按UTF-8字节数截断字符串（避免截断半个中文）
        /// </summary>
        /// <param name="str">要截断的字符串</param>
        /// <param name="maxBytes">最大字节数</param>
        /// <returns>截断后的字符串</returns>
        private static string TruncateByByteLength(string str, int maxBytes)
        {
            if (string.IsNullOrEmpty(str)) return str;

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            if (bytes.Length <= maxBytes) return str;

            // 从后往前截断，直到字节数≤maxBytes（避免半个中文）
            for (int i = str.Length - 1; i >= 0; i--)
            {
                string truncated = str.Substring(0, i + 1);
                if (Encoding.UTF8.GetByteCount(truncated) <= maxBytes)
                {
                    return truncated;
                }
            }

            // 极端情况（单个字符就超字节数），返回空字符串
            return "";
        }
        #endregion




        // 清理路径中的特殊字符（避免创建文件夹失败）
        public static string SanitizePath(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = "其他";
                }

                // 步骤1：移除话题标签（#xxx 或 #xxx#yyy）
                //path = Regex.Replace(path, @"#\S+", "", RegexOptions.Compiled);

                // 步骤2：移除表情符号（匹配常见表情Unicode区块）
                //string emojiPattern = @"[\u1F600-\u1F64F\u1F300-\u1F5FF\u1F680-\u1F6FF\u1E000-\u1EFFF\u2600-\u2B55\u200D]";
                //path = Regex.Replace(path, emojiPattern, "", RegexOptions.Compiled);
                path = Regex.Replace(path, @"[^a-zA-Z0-9\u4e00-\u9fa5]", "-");

                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    path = path.Replace(c, '_');
                }
                if (path.Length > 100)
                {
                    path = path.Substring(0, 100);
                }
                return path.Trim().Replace(" ", "");
            }
            catch (Exception ex)
            {
                return path;
            }
        }


        /// <summary>
        /// 检查字符串是否仅包含字母、数字、简体中文（无特殊字符）
        /// </summary>
        /// <param name="input">待检查的字符串</param>
        /// <returns>true：无特殊字符（仅允许字符）；false：含有特殊字符</returns>
        public static bool IsValidWithoutSpecialChars(string input)
        {
            // 空字符串默认返回 true（若需禁止空字符串，可先判断 string.IsNullOrWhiteSpace(input) 并返回 false）
            if (string.IsNullOrEmpty(input))
                return true;

            // 正则表达式说明：
            // ^ ：匹配字符串开头
            // $ ：匹配字符串结尾
            // [a-zA-Z0-9\u4E00-\u9FA5] ：允许的字符范围
            //   a-zA-Z：大小写字母
            //   0-9：数字
            //   \u4E00-\u9FA5：简体中文 Unicode 核心范围（覆盖99%+简体中文常用字）
            // * ：匹配 0 个或多个允许的字符（若需至少1个字符，可改为 +）
            const string pattern = @"^[a-zA-Z0-9\u4E00-\u9FA5]*$";

            // 忽略文化差异，仅按字符编码匹配
            return Regex.IsMatch(input, pattern, RegexOptions.None);
        }
    }
}
