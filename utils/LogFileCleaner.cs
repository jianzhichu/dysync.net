namespace dy.net.utils
{
    public class LogFileCleaner
    {
        /// <summary>
        /// 清除指定目录下n天之前的日志文件
        /// </summary>
        /// <param name="logDirectory">日志文件所在目录</param>
        /// <param name="n">清除N天之前的日志文件</param>
        public static void CleanOldLogFiles(string logDirectory,int n)
        {
            // 计算n天前的时间点
            DateTime threeDaysAgo = DateTime.Now.AddDays(-n);

            // 遍历日志目录下的所有文件
            foreach (string file in Directory.GetFiles(logDirectory))
            {
                FileInfo fileInfo = new FileInfo(file);
                // 判断文件修改时间是否早于3天前
                if (fileInfo.LastWriteTime < threeDaysAgo)
                {
                    try
                    {
                        // 删除符合条件的文件
                        File.Delete(file);
                        Console.WriteLine($"已删除文件: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"删除文件 {file} 时发生异常: {ex.Message}");
                    }
                }
            }
        }
    }
}
