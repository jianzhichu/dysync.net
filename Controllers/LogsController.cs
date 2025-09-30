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

        public LogsController(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetLog(string type, string date)
        {
            var filePath = Path.Combine(webHostEnvironment.IsDevelopment() ? Directory.GetCurrentDirectory() : AppDomain.CurrentDomain.BaseDirectory, "logs", $"log-{type}-{date}.txt");
            if (!System.IO.File.Exists(filePath))
            {
                var msg = $"Log file log-{type}-{date}.txt not found.";
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


    }
}
