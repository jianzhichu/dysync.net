using dy.net.dto;
using System.Globalization;

namespace dy.net.service
{
    public class LogInfoService
    {

        public List<LogFileInfoDto> GetLogFiles(string logDirectory)
        {
            var logFiles = new List<LogFileInfoDto>();

            // 验证目录是否存在
            if (!Directory.Exists(logDirectory))
            {
                Serilog.Log.Error("日志目录不存在：{Directory}", logDirectory);
                return logFiles;
            }

            // 获取最近10天的日期范围
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-9);

            // 遍历日期范围，查找对应日志文件
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dateStr = date.ToString("yyyyMMdd");

                // 查找debug日志
                var debugFile = Path.Combine(logDirectory, $"log-debug-{dateStr}.txt");
                if (File.Exists(debugFile))
                {
                    logFiles.Add(new LogFileInfoDto
                    {
                        Type = "debug",
                        Date = dateStr,
                        FileName = Path.GetFileName(debugFile)
                    });
                }

                // 查找error日志
                var errorFile = Path.Combine(logDirectory, $"log-error-{dateStr}.txt");
                if (File.Exists(errorFile))
                {
                    logFiles.Add(new LogFileInfoDto
                    {
                        Type = "error",
                        Date = dateStr,
                        FileName = Path.GetFileName(errorFile)
                    });
                }
            }

            return logFiles.OrderByDescending(x => x.Date).ToList();
        }


        public Stream GetLogFileStream(string logDirectory, string type, string date)
        {
            // 验证日期格式
            if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _))
            {
                throw new ArgumentException("日期格式错误，应为yyyyMMdd");
            }

            // 验证类型
            if (type != "debug" && type != "error")
            {
                throw new ArgumentException("日志类型只能是debug或error");
            }

            var filePath = Path.Combine(logDirectory, $"log-{type}-{date}.txt");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("日志文件不存在", filePath);
            }

            // 返回文件流（使用FileStream确保流可被前端读取）
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}
