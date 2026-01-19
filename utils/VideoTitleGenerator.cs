using dy.net.model.dto;
using System.Text.RegularExpressions;

namespace dy.net.utils
{
    /// <summary>
    /// 视频标题模板生成器
    /// </summary>
    public static class VideoTitleGenerator
    {
        /// <summary>
        /// 根据用户模板和原始数据生成最终标题
        /// </summary>
        /// <param name="template">用户设置的模板（如 "视频_{id}_{VideoTitle}_{ReleaseTime}"）</param>
        /// <param name="data">标题所需的原始数据</param>
        /// <param name="timeFormat">时间字段格式化（默认：yyyy-MM-dd HH:mm:ss）</param>
        /// <param name="emptyPlaceholder">占位符对应数据为空时的替换值（默认：空字符串）</param>
        /// <returns>生成的最终标题</returns>
        /// <exception cref="ArgumentNullException">模板为空时抛出</exception>
        public static string Generate(
            string template,
            VideoTitleDataTemplate data,
            string timeFormat = "yyyyMMdd",
            string emptyPlaceholder = "")
        {
            // 校验入参
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentNullException(nameof(template), "标题模板不能为空");
            data ??= new VideoTitleDataTemplate(); // 避免数据为空

            // 1. 定义占位符与数据的映射关系（key：占位符名称，value：格式化后的值）
            var placeholderMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // 普通字段
                ["Id"] = data.Id.ToString(),
                ["VideoTitle"] = string.IsNullOrWhiteSpace(DouyinFileNameHelper.KeepChineseLettersAndNumbers(data.VideoTitle)) ? emptyPlaceholder : DouyinFileNameHelper.KeepChineseLettersAndNumbers(data.VideoTitle),
                ["FileHash"] = string.IsNullOrWhiteSpace(data.FileHash) ? emptyPlaceholder : data.FileHash,
                ["Resolution"] = string.IsNullOrWhiteSpace(data.Resolution) ? emptyPlaceholder : data.Resolution,
                ["Author"] = string.IsNullOrWhiteSpace(DouyinFileNameHelper.KeepChineseLettersAndNumbers(data.Author)) ? emptyPlaceholder : DouyinFileNameHelper.KeepChineseLettersAndNumbers(data.Author),

                // 时间字段（支持空值处理）
                //["SyncTime"] = data.SyncTime.HasValue ? data.SyncTime.Value.ToString(timeFormat) : emptyPlaceholder,
                ["ReleaseTime"] = data.ReleaseTime.HasValue ? data.ReleaseTime.Value.ToString(timeFormat) : emptyPlaceholder,

                // 文件大小（自动格式化：字节→KB/MB/GB，保留1位小数）
                //["FileSize"] = FormatFileSize(data.FileSize) ?? emptyPlaceholder
            };

            // 2. 正则匹配模板中的占位符（{占位符名称}），并替换
            var regex = new Regex(@"\{(?<key>[a-zA-Z0-9]+)\}", RegexOptions.Compiled);
            var finalTitle = regex.Replace(template, match =>
            {
                var placeholderKey = match.Groups["key"].Value;
                // 存在对应映射则替换，否则保留原占位符（避免替换错误）
                return placeholderMap.TryGetValue(placeholderKey, out var value) ? value : match.Value;
            });

            var fullName= finalTitle.Replace("--","-");

            if (fullName.Length > 60)
            {
                return fullName.Substring(0, 60);
            }
            else
            {
                return fullName;
            }
        }
    }
}
