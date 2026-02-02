using System.Text;
using System.Text.RegularExpressions;

namespace dy.net.utils
{
    /// <summary>
    /// 抖音标题转文件名工具类（兼容Windows/macOS/Linux）
    /// </summary>
    public static class DouyinFileNameHelper
    {
        /// <summary>
        /// 处理文件名/文件夹名，确保符合 Linux 限制（最大 255 字节 UTF-8 编码）
        /// </summary>
        /// <param name="originalName">原始名称（支持英文、中文、混合字符）</param>
        /// <param name="defaultName">截取后为空时的默认名称（默认 "default"）</param>
        /// <param name="isfolder">是否是创建文件夹 命名</param>
        /// <returns>符合 Linux 规则的合法名称</returns>
        public static string SanitizeLinuxFileName(string originalName, string defaultName, bool isfolder = false)
        {
            string result = string.Empty;
            // 1. 空值处理：直接返回默认名
            if (string.IsNullOrWhiteSpace(originalName))
                result = defaultName.Replace(" ", "");
            else
            {
                // 2. 过滤 Linux 非法字符：
                // - 禁止：/（路径分隔符）、\0（空字符）
                // - 替换：其他特殊字符（如 :*?"<>|\\ ）为下划线 _，避免创建失败
                //var invalidChars = new[] { '/', '\0', ':', '*', '?', '"', '<', '>', '|', '\\' };

                var invalidChars = new[] {
                '/', '\0',    // Linux 核心非法字符
                '\n', '\r', '\v', '\f',  // 所有换行/回车/制表类控制字符
                ':', '*', '?', '"', '<', '>', '|', '\\'  // 其他常见非法字符
                };
                string sanitizedName = originalName;
                foreach (var c in invalidChars)
                {
                    sanitizedName = sanitizedName.Replace(c, '_');
                }

                // 3. 计算 UTF-8 字节数，若未超 255 字节，直接返回
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(sanitizedName);
                if (utf8Bytes.Length <= 100)
                    result = sanitizedName.Replace(" ", "");
                else
                {
                    // 4. 超过 255 字节，截取前 255 字节（避免破坏 UTF-8 字符）
                    byte[] truncatedBytes = new byte[100];
                    Array.Copy(utf8Bytes, truncatedBytes, 100);

                    // 5. 字节数组转回字符串（自动忽略不完整的尾部字节，避免乱码）
                    string truncatedName = Encoding.UTF8.GetString(truncatedBytes).TrimEnd('\0').Replace(" ", ""); // 移除可能的空字符

                    // 6. 极端情况：截取后为空（如全是非法字符替换后无有效内容），返回默认名
                    result = string.IsNullOrWhiteSpace(truncatedName) ? defaultName : truncatedName;
                }
                if (isfolder)
                {
                    result = KeepChineseLettersAndNumbers(result);
                }
            }
            if (string.IsNullOrWhiteSpace(result))
                result = defaultName;
            return result;
        }



        /// 保留字符串中的中文、字母、数字，去除其他所有字符
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <returns>处理后的字符串</returns>
        public static string KeepChineseLettersAndNumbers(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input; // 空字符串/Null直接返回，避免异常

            // 正则表达式：匹配非中文（\u4e00-\u9fa5）、非字母（a-zA-Z）、非数字（0-9）的字符
            string pattern = @"[^\u4e00-\u9fa5a-zA-Z0-9]";
            return Regex.Replace(input, pattern, string.Empty);
        }


        /// 检查字符串是否仅包含字母、数字、简体中文（无特殊字符）
        /// </summary>
        /// <param name="input">待检查的字符串</param>
        /// <returns>true：无特殊字符（仅允许字符）；false：含有特殊字符</returns>
        public static bool IsValidWithoutSpecialChars(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return true;

            // 正则表达式说明：
            // ^ ：匹配字符串开头
            // $ ：匹配字符串结尾
            // [a-zA-Z0-9\u4E00-\u9FA5 _] ：允许的字符范围（新增空格和下划线）
            //   a-zA-Z：大小写字母
            //   0-9：数字
            //   \u4E00-\u9FA5：简体中文 Unicode 核心范围
            //   空格：普通空格字符
            //   _：下划线
            // * ：匹配 0 个或多个允许的字符（若需至少1个字符，可改为 +）
            const string pattern = @"^[a-zA-Z0-9\u4E00-\u9FA5 _]*$";

            // 忽略文化差异，仅按字符编码匹配
            return Regex.IsMatch(input, pattern, RegexOptions.None);
        }


        /// <summary>
        /// 去掉动态视频001_002
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string RemoveNumberSuffix(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;
            // 核心正则：只匹配「_+数字」且后面紧跟.的情况
            var pattern = @"_\d+(?=\.)";
            return Regex.Replace(fileName, pattern, "");
        }


        /// <summary>
        /// 中英文混合截断：按视觉格子数限制（中文/全角符=2格，英文/半角符/数字=1格）
        /// </summary>
        /// <param name="inputStr">待处理字符串（路径/名称/备注都可）</param>
        /// <param name="maxVisualGrid">总视觉格子数（如20，核心参数）</param>
        /// <param name="addEllipsis">是否加省略号（UI展示=true，存储=false）</param>
        /// <returns>视觉长度合规的字符串</returns>
        public static string LimitUnifiedCount(this string inputStr, int maxTotalCount, bool addEllipsis = false)
        {
            // 边界防护：空字符串直接返回；传0/负数则按1处理，传20则正常取20（不影响核心需求）
            if (string.IsNullOrEmpty(inputStr)) return inputStr;
            int validMaxCount = Math.Max(maxTotalCount, 1);

            // 总数未超，直接返回原字符串
            if (inputStr.Length <= validMaxCount) return inputStr;

            // 总数超了，截取前N个（传20则截前20，纯中文能留20个）
            StringBuilder sb = new StringBuilder(inputStr.Substring(0, validMaxCount));
            // 截断后加省略号（UI展示用）
            if (addEllipsis) sb.Append("...");

            return sb.ToString();
        }

    }
}