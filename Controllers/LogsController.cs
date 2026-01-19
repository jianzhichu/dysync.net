using dy.net.model.dto;
using dy.net.service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace dy.net.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly LogInfoService logInfoService;

        public LogsController(IWebHostEnvironment webHostEnvironment,LogInfoService logInfoService)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.logInfoService = logInfoService;
        }

        [HttpGet("/api/logs/GetLog/{type}/{date}")]
        public async Task<IActionResult> GetLog([FromRoute]string type, [FromRoute] string date)
        {
            var filePath = Path.Combine(webHostEnvironment.IsDevelopment() ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory, "logs", $"log-{type}-{date}.txt");
            if (!System.IO.File.Exists(filePath))
            {
                var msg = $"{date}，没有发现{type}的日志";
                //Serilog.Log.Error(msg);
                return Ok(msg);
            }
            return PhysicalFile(filePath, "text/plain; charset=utf-8");

            //下面的方案提示文件被占用
            //var encoding = Encoding.GetEncoding("UTF-8"); // 指定文本文件的编码
            //var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            //var fileContent = encoding.GetString(fileBytes);
            //return  Content (fileContent, "text/plain", encoding);
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
