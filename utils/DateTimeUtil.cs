namespace dy.net.utils
{
    public class DateTimeUtil
    {
        public static DateTime Convert10BitTimestamp(long timestamp)
        {
            // 1. 定义Unix时间戳的起始时间（1970-01-01 UTC）
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // 2. 计算目标时间（UTC时区）：起始时间 + 时间戳（秒数）
            DateTime utcTime = epoch.AddSeconds(timestamp);

            // 3. 转换为本地时间（可选，根据需求选择UTC或本地时间）
            DateTime localTime = utcTime.ToLocalTime();
            return localTime;
            // 4. 格式化为yyyy-MM-dd HH:mm:ss
            //return localTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 获取当前Unix时间戳（毫秒级）
        /// </summary>
        /// <returns>毫秒级时间戳（long类型）</returns>
        public static long GetUnixTimestampMilliseconds()
        {
            DateTimeOffset epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            // 计算当前UTC时间与纪元时间的差值（毫秒）
            long timestamp = (long)(DateTimeOffset.UtcNow - epoch).TotalMilliseconds;
            return timestamp;
        }
    }
}
