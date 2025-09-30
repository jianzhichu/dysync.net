namespace dy.net.utils
{
    public static class CookieValidator
    {
        /// <summary>
        /// 粗略验证Cookie字符串的格式是否合法
        /// 核心校验规则：
        /// 1. 不为空或纯空白字符串
        /// 2. 由分号分隔的键值对组成（键值对格式为 key=value，key不能为空）
        /// 3. 键名（key）不包含非法字符（分号、逗号、空格、等号）
        /// 注：此方法为粗略验证，不严格遵循RFC 6265标准（如未校验控制字符、长度限制等）
        /// </summary>
        /// <param name="cookieStr">待验证的Cookie字符串</param>
        /// <returns>格式是否合法（true=合法，false=非法）</returns>
        public static bool IsRoughlyValidCookieFormat(string cookieStr)
        {
            // 规则1：Cookie字符串不能为空或纯空白
            if (string.IsNullOrWhiteSpace(cookieStr))
            {
                Serilog.Log.Error("验证失败：Cookie字符串为空或纯空白");
                return false;
            }

            // 按分号分隔Cookie项（处理项前后的空格，过滤空项）
            var cookieItems = cookieStr.Split(';')
                .Select(item => item.Trim()) // 去除项前后空格（如 "a=b;  c=d" → ["a=b", "c=d"]）
                .Where(item => !string.IsNullOrWhiteSpace(item)) // 过滤空项（如末尾分号导致的空项）
                .ToList();

            // 若分割后无有效项（如全是分号或空格）
            if (!cookieItems.Any())
            {
                Serilog.Log.Error("验证失败：Cookie字符串仅包含分隔符或空格");
                return false;
            }

            // 定义键名（key）的非法字符（粗略验证，选取最常见的非法字符）
            char[] invalidKeyChars = { ';', ',', ' ', '=' };

            // 遍历每个Cookie项，验证键值对格式
            foreach (var item in cookieItems)
            {
                // 规则2：每个项必须包含等号（=），且等号不能是第一个字符（保证key非空）
                int equalsIndex = item.IndexOf('=');
                if (equalsIndex <= 0) // equalsIndex=0 → 以等号开头（key为空）；equalsIndex=-1 → 无等号
                {
                    Serilog.Log.Error($"验证失败：Cookie项 [{item}] 缺少等号或键名为空");
                    return false;
                }

                // 提取键名（key）并验证
                string key = item.Substring(0, equalsIndex).Trim(); // 再次Trim以防key前后有空格（如 "  key  =value"）

                // 规则3：键名不能包含非法字符
                if (key.Any(c => invalidKeyChars.Contains(c)))
                {
                    Serilog.Log.Error($"验证失败：Cookie项 [{item}] 的键名 [{key}] 包含非法字符（; , 空格 =）");
                    return false;
                }

                // （可选）粗略验证value：此处不做严格限制，允许value为空（如 "key="）或包含特殊字符
            }

            // 所有项均通过验证
            //Serilog.Log.Error("Cookie格式粗略验证通过");
            return true;
        }
    }
}
