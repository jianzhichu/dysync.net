using dy.net.model.dto;
using dy.net.service;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace dy.net.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        // JSON 日志接口保持兼容，但最多缓冲最后 4 MiB，避免大日志产生无界字符串分配。
        private const long MaxBufferedLogBytes = 4L * 1024 * 1024;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly LogInfoService logInfoService;

        public LogsController(IWebHostEnvironment webHostEnvironment, LogInfoService logInfoService)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.logInfoService = logInfoService;
        }
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpGet("/api/logs/GetLog/{type}/{date}")]
        public async Task<IActionResult> GetLog(
            [FromRoute] string type,
            [FromRoute] string date)
        {
            DisableClientCache();

            var logDirectory = Path.Combine(
                webHostEnvironment.IsDevelopment()
                    ? Directory.GetCurrentDirectory()
                    : AppDomain.CurrentDomain.BaseDirectory,
                "logs");

            try
            {
                await using var stream =
                    logInfoService.GetLogFileStream(logDirectory, type, date);

                var truncated = false;
                if (stream.CanSeek && stream.Length > MaxBufferedLogBytes)
                {
                    stream.Seek(-MaxBufferedLogBytes, SeekOrigin.End);
                    truncated = true;
                }

                using var reader = new StreamReader(
                    stream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 8192,
                    leaveOpen: false);

                var content = await reader.ReadToEndAsync();

                if (truncated)
                {
                    // 从下一行开始，避免截断点落在 UTF-8 字符或半条日志中间。
                    var firstLineEnd = content.IndexOf('\n');
                    if (firstLineEnd >= 0 && firstLineEnd + 1 < content.Length)
                    {
                        content = content[(firstLineEnd + 1)..];
                    }
                    content = "[日志文件过大，仅显示最后 4 MiB]" + Environment.NewLine + content;
                }

                // 使用统一响应结构，避免前端响应拦截器把纯文本判为异常。
                return ApiResult.Success<string>(content);
            }
            catch (FileNotFoundException)
            {
                return ApiResult.Success<string>($"{date}，没有发现{type}的日志");
            }
            catch (ArgumentException ex)
            {
                return ApiResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(
                    ex,
                    "读取日志失败，Type={Type}, Date={Date}",
                    type,
                    date);

                return ApiResult.Fail("读取日志失败：" + ex.Message);
            }
        }

        private void DisableClientCache()
        {
            Response.Headers["Cache-Control"] =
                "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
        }



        /// <summary>
        /// 获取最近10天的日志文件列表
        /// </summary>
        [HttpGet("/api/logs/list")]
        public IActionResult GetLogFiles()
        {
            try
            {
                var _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                var files = logInfoService.GetLogFiles(_logDirectory);
                return ApiResult.Success(files);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail("获取日志列表失败," + ex.Message);
            }
        }



        /// <summary>
        /// 获取日志文件内容流
        /// </summary>
        [HttpGet("/api/logs/content")]
        public IActionResult GetLogContent([FromQuery] LogContentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                var stream = logInfoService.GetLogFileStream(_logDirectory, request.Type, request.Date);
                // 返回文件流，指定MIME类型为文本
                return File(stream, "text/plain", $"log-{request.Type}-{request.Date}.txt");
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "读取日志文件失败", error = ex.Message });
            }
        }
    }
}
