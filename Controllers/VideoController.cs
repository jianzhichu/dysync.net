using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using System.Drawing.Printing;
using System.Threading.Tasks;

namespace dy.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly DouyinVideoService dyCollectVideoService;
        private readonly DouyinCommonService douyinCommonService;

        public VideoController(DouyinVideoService dyCollectVideoService, DouyinCommonService douyinCommonService)
        {
            this.dyCollectVideoService = dyCollectVideoService;
            this.douyinCommonService = douyinCommonService;
        }
        /// <summary>
        /// 分页查询收藏视频
        /// </summary>
        /// <param name="dto"></param>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedAsync(DouyinVideoPageRequestDto dto)
        {
            var (list, totalCount) = await dyCollectVideoService.GetPagedAsync(dto);
            return Ok(new
            {
                code = 0,
                data = new
                {
                    data = list,
                    total=totalCount,
                    pageIndex=dto.PageIndex,
                    pageSize=dto.PageSize
                }
            });
        }

        /// <summary>
        /// 查询统计数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("statics")]
        public async Task<IActionResult> GetStaticsAsync()
        {
            var data = await dyCollectVideoService.GetStatics();
            return Ok(new
            {
                code = 0,
                data
            });
        }

        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("play/{vid}")]
        public async Task<IActionResult> StreamVideo([FromRoute] string vid)
        {
            try
            {
                var viedo = await dyCollectVideoService.GetById(vid);

                if (viedo == null)
                {
                    return NotFound($"视频不存在：{vid}");
                }
                return PlayViedo(viedo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"视频加载失败：{ex.Message}");
            }
        }

        private IActionResult PlayViedo(DouyinVideo viedo)
        {

            // 1. 拼接完整物理路径（配置路径 + 文件名）
            string videoFullPath = viedo.VideoSavePath;

            // 2. 验证文件是否存在
            if (!System.IO.File.Exists(videoFullPath))
            {
                return NotFound($"视频文件不存在：{videoFullPath}");
            }

            // 3. 获取文件信息（大小、类型）
            var fileInfo = new FileInfo(videoFullPath);
            long fileSize = fileInfo.Length;
            string contentType = GetContentType(videoFullPath);  // 自动识别视频 MIME 类型

            // 4. 处理分片请求（前端视频标签自动发起，支持断点续传）
            if (Request.Headers.ContainsKey("Range") && long.TryParse(Request.Headers.Range.ToString().Split('=')[1].Split('-')[0], out long start))
            {
                // 分片起始位置（前端请求的起始字节）
                long end = Math.Min(start + 1024 * 1024 * 2, fileSize - 1);  // 每片 2MB（可调整）
                long chunkSize = end - start + 1;

                // 5. 设置分片响应头
                Response.StatusCode = StatusCodes.Status206PartialContent;
                Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileSize}");
                Response.Headers.Add("Accept-Ranges", "bytes");
                Response.Headers.Add("Content-Length", chunkSize.ToString());

                // 6. 读取分片并返回流
                var stream = new FileStream(videoFullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                stream.Seek(start, SeekOrigin.Begin);
                return new FileStreamResult(stream, contentType);
            }
            else
            {
                // 完整文件请求（兼容旧浏览器）
                return PhysicalFile(videoFullPath, contentType, enableRangeProcessing: true);
            }
        }


        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        [HttpGet("/share/{vid}/{k}")]
        [AllowAnonymous]
        public async Task<IActionResult> Share([FromRoute] string vid, [FromRoute] string k)
        {
            try
            {
                var viedo = await dyCollectVideoService.GetById(vid);

                if (viedo == null)
                {
                    return NotFound($"视频不存在：{vid}");
                }

                var expectedKey = (viedo.FileHash + viedo.AuthorId).Md5();
                if (expectedKey != k)
                {
                    return NotFound($"视频地址无效");
                }

                return PlayViedo(viedo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"视频加载失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 辅助方法：根据文件名获取 MIME 类型（确保前端正确识别视频格式）
        /// </summary>
        private string GetContentType(string filename)
        {
            string extension = Path.GetExtension(filename).ToLowerInvariant();
            return extension switch
            {
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".ogg" => "video/ogg",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                _ => "application/octet-stream"  // 默认二进制流
            };
        }


        /// <summary>
        /// 重新下载
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost("redown")]
        public async Task<IActionResult> ReDownload(ReDownViedoDto dto)
        {
            if (dto == null)
            {
                return Ok(new { code = -1, data = false });
            }
            else
            {
                var result = await dyCollectVideoService.ReDownloadViedoAsync(dto);
                if (result)
                {
                    return Ok(new { code = 0, data = true });
                }
                else
                {
                    return Ok(new { code = -1, data = false });
                }
            }
        }
        /// <summary>
        /// 删除视频-不再下载
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("vdelete/{vid}")]
        public async Task<IActionResult> DeleteVideo([FromRoute]string vid)
        {
            if (string.IsNullOrWhiteSpace(vid))
            {
                return Ok(new { code = -1, data = false });
            }
            else
            {
                var video = await dyCollectVideoService.GetById(vid);
                if (video == null)
                {
                    return Ok(new { code = -1, data = false });
                }
                else
                {
                    var result = await dyCollectVideoService.ReDownloadViedoAsync(new ReDownViedoDto { Ids = new List<string> { vid } });
                    if (result)
                    {
                        //加入删除逻辑
                        await douyinCommonService.AddDeleteVideo(new DouyinVideoDelete { ViedoId = video.AwemeId });
                        Serilog.Log.Debug($"前面的日志,你错了，这条视频是永久删除..哈哈--{video.VideoTitle}");

                        return Ok(new { code = 0, data = true });
                    }
                    else
                    {
                        return Ok(new { code = -1, data = false });
                    }

                }
              
            }
        }
    }
}
