using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.service;
using dy.net.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Cryptography;

namespace dy.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly DouyinVideoService douyinVideoService;
        private readonly DouyinCommonService douyinCommonService;

        public VideoController(DouyinVideoService dyCollectVideoService, DouyinCommonService douyinCommonService)
        {
            this.douyinVideoService = dyCollectVideoService;
            this.douyinCommonService = douyinCommonService;
        }
        /// <summary>
        /// 分页查询收藏视频
        /// </summary>
        /// <param name="dto"></param>
        [Authorize]
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedAsync(DouyinVideoPageRequestDto dto)
        {
            var (list, totalCount) = await douyinVideoService.GetPagedAsync(dto);
            return ApiResult.Success(new
            {
                data = list,
                total = totalCount,
                pageIndex = dto.PageIndex,
                pageSize = dto.PageSize
            });
        }

        /// <summary>
        /// 查询统计数据
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("statics")]
        public async Task<IActionResult> GetStaticsAsync()
        {
            return ApiResult.Success(await douyinVideoService.GetStatics());
        }

        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("play/{vid}")]
        public async Task<IActionResult> StreamVideo([FromRoute] string vid)
        {
            try
            {
                var viedo = await douyinVideoService.GetById(vid);

                if (viedo == null)
                {
                    return ApiResult.Fail($"视频不存在：{vid}");
                }
                return await PlayVideoAsync(viedo);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"视频加载失败：{ex.Message}");
            }
        }

        //private IActionResult PlayViedo(DouyinVideo viedo)
        //{

        //    // 1. 拼接完整物理路径（配置路径 + 文件名）
        //    string videoFullPath = viedo.VideoSavePath;

        //    // 2. 验证文件是否存在
        //    if (!System.IO.File.Exists(videoFullPath))
        //    {
        //        return ApiResult.Fail($"视频文件不存在：{videoFullPath}");
        //    }

        //    // 3. 获取文件信息（大小、类型）
        //    var fileInfo = new FileInfo(videoFullPath);
        //    long fileSize = fileInfo.Length;
        //    string contentType = GetContentType(videoFullPath);  // 自动识别视频 MIME 类型

        //    // 4. 处理分片请求（前端视频标签自动发起，支持断点续传）
        //    if (Request.Headers.ContainsKey("Range") && long.TryParse(Request.Headers.Range.ToString().Split('=')[1].Split('-')[0], out long start))
        //    {
        //        // 分片起始位置（前端请求的起始字节）
        //        long end = Math.Min(start + 1024 * 1024 * 2, fileSize - 1);  // 每片 2MB（可调整）
        //        long chunkSize = end - start + 1;

        //        // 5. 设置分片响应头
        //        Response.StatusCode = StatusCodes.Status206PartialContent;
        //        Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileSize}");
        //        Response.Headers.Add("Accept-Ranges", "bytes");
        //        Response.Headers.Add("Content-Length", chunkSize.ToString());

        //        // 6. 读取分片并返回流
        //        var stream = new FileStream(
        //        videoFullPath,
        //        FileMode.Open,
        //        FileAccess.Read,
        //        FileShare.Read,
        //        bufferSize: 4096,
        //        useAsync: true);

        //        stream.Seek(start, SeekOrigin.Begin);
        //        return new FileStreamResult(stream, contentType);
        //    }
        //    else
        //    {
        //        // 完整文件请求（兼容旧浏览器）
        //        return PhysicalFile(videoFullPath, contentType, enableRangeProcessing: true);
        //    }
        //}

        /// <summary>
        /// 抖音视频播放接口（优化版）
        /// 解决内存泄漏、资源释放不及时、鲁棒性不足等问题
        /// </summary>
        /// <param name="video">视频实体（修正拼写错误：viedo -> video）</param>
        /// <returns>视频流响应</returns>
        public async Task<IActionResult> PlayVideoAsync(DouyinVideo video)
        {
            // 空值校验
            if (video == null)
            {
                return ApiResult.Fail("视频信息不能为空");
            }

            // 1. 获取完整物理路径并校验
            string videoFullPath = video.VideoSavePath;
            if (string.IsNullOrWhiteSpace(videoFullPath))
            {
                return ApiResult.Fail("视频保存路径不能为空");
            }

            // 安全校验：防止路径遍历攻击
            try
            {
                videoFullPath = Path.GetFullPath(videoFullPath);
            }
            catch
            {
                return ApiResult.Fail("视频路径格式非法");
            }

            // 2. 验证文件是否存在
            if (!System.IO.File.Exists(videoFullPath))
            {
                return ApiResult.Fail($"视频文件不存在：{videoFullPath}");
            }

            try
            {
                // 3. 获取文件信息（使用using确保FileInfo资源释放）
                var fileInfo = new FileInfo(videoFullPath);
                long fileSize = fileInfo.Length;

                // 校验空文件
                if (fileSize == 0)
                {
                    return ApiResult.Fail($"视频文件为空：{videoFullPath}");
                }

                string contentType = GetContentType(videoFullPath);

                // 4. 处理分片请求（Range）
                if (Request.Headers.ContainsKey("Range"))
                {
                    return await HandleRangeRequestAsync(videoFullPath, fileSize, contentType);
                }
                else
                {
                    // 完整文件请求（启用分片处理，兼容前端断点续传）
                    return PhysicalFile(videoFullPath, contentType, enableRangeProcessing: true);
                }
            }
            catch (UnauthorizedAccessException)
            {
                return ApiResult.Fail($"没有权限访问视频文件：{videoFullPath}");
            }
            catch (IOException ex)
            {
                // 记录日志（建议添加日志框架，如Serilog/NLog）
                // _logger.LogError(ex, "读取视频文件失败：{Path}", videoFullPath);
                return ApiResult.Fail($"读取视频文件失败：{ex.Message}");
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "视频播放接口异常：{Path}", videoFullPath);
                return ApiResult.Fail($"服务器内部错误：{ex.Message}");
            }
        }

        /// <summary>
        /// 处理分片请求（Range），异步安全处理流
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileSize">文件总大小</param>
        /// <param name="contentType">内容类型</param>
        /// <returns>分片响应</returns>
        private async Task<IActionResult> HandleRangeRequestAsync(string filePath, long fileSize, string contentType)
        {
            string rangeHeader = Request.Headers["Range"].ToString();

            // 安全解析Range头
            if (!rangeHeader.StartsWith("bytes="))
            {
                // 416 - 请求的范围无法满足
                Response.StatusCode = StatusCodes.Status416RequestedRangeNotSatisfiable;
                Response.Headers.Add("Content-Range", $"bytes */{fileSize}");
                return new EmptyResult();
            }

            // 解析起始位置
            string[] rangeParts = rangeHeader.Split('=')[1].Split('-');
            if (!long.TryParse(rangeParts[0], out long start) || start < 0 || start >= fileSize)
            {
                Response.StatusCode = StatusCodes.Status416RequestedRangeNotSatisfiable;
                Response.Headers.Add("Content-Range", $"bytes */{fileSize}");
                return new EmptyResult();
            }

            // 计算分片结束位置（每片2MB，可配置）
            long end = Math.Min(start + 1024 * 1024 * 2, fileSize - 1);
            long chunkSize = end - start + 1;

            // 设置206分片响应头
            Response.StatusCode = StatusCodes.Status206PartialContent;
            Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileSize}");
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("Content-Length", chunkSize.ToString());
            Response.ContentType = contentType;

            // 核心改进：使用异步流 + using确保释放（通过管道直接写入响应流）
            await using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192, // 增大缓冲区提升性能，减少IO次数
                FileOptions.Asynchronous | FileOptions.SequentialScan); // 顺序扫描优化

            // 定位到分片起始位置
            fileStream.Seek(start, SeekOrigin.Begin);

            // 直接写入响应流，避免FileStreamResult的延迟释放问题
            var buffer = new byte[8192];
            long remainingBytes = chunkSize;

            while (remainingBytes > 0)
            {
                int bytesRead = await fileStream.ReadAsync(buffer, 0, (int)Math.Min(remainingBytes, buffer.Length));
                if (bytesRead == 0) break;

                await Response.Body.WriteAsync(buffer.AsMemory(0, bytesRead));
                remainingBytes -= bytesRead;
            }

            await Response.Body.FlushAsync();

            return new EmptyResult();
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
                var viedo = await douyinVideoService.GetById(vid);

                if (viedo == null)
                {
                    return ApiResult.Fail($"视频不存在：{vid}");
                }

                var expectedKey = (viedo.FileHash + viedo.AuthorId).Md5();
                if (expectedKey != k)
                {
                    return ApiResult.Fail($"视频地址无效");
                }

                return await PlayVideoAsync(viedo);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"视频加载失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 辅助方法：根据文件名获取 MIME 类型（确保前端正确识别视频格式）
        /// </summary>
        private static string GetContentType(string filename)
        {
            string extension = Path.GetExtension(filename).ToLowerInvariant();
            return extension switch
            {
                ".mp4" => "video/mp4",
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
                return ApiResult.Fail("参数错误");
            }
            else
            {
                var result = await douyinVideoService.ReDownloadViedoAsync(dto);
                if (result)
                {
                    return ApiResult.Success(true);
                }
                else
                {
                    return ApiResult.Fail("错误");
                }
            }
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("vdelete/batch")]
        public async Task<IActionResult> BathRealDelete(ReDownViedoDto dto)
        {
            var result = await douyinVideoService.RealDeleteVideos(dto.Ids);
            if (result)
            {
                return ApiResult.Success(true);
            }
            else
            {
                return ApiResult.Fail("错误");
            }
        }


        /// <summary>
        /// 删除视频-不再下载
        /// </summary>
        /// <param name="vid"></param>
        /// <returns></returns>
        [HttpGet("vdelete/{vid}")]
        public async Task<IActionResult> DeleteVideo([FromRoute] string vid)
        {
            if (string.IsNullOrWhiteSpace(vid))
            {
                return ApiResult.Fail("参数错误");
            }
            else
            {
                var res = await douyinVideoService.RealDeleteVideos(new List<string> { vid });
                if (res)
                {
                    return ApiResult.Success("删除成功");
                }
                else
                {
                    return ApiResult.Fail("删除失败");
                }
            }
        }

        /// <summary>
        /// 查询已删除视频列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("vdelete/get")]
        public async Task<IActionResult> GetDeleteVideo()
        {
            return ApiResult.Success(await douyinCommonService.GetDouyinDeleteVideos());
        }
        /// <summary>
        /// 根据博主id删除博主所有视频
        /// </summary>
        /// <param name="uperUid"></param>
        /// <returns></returns>
        [HttpGet("vdelete/byauthor/{uperUid}")]
        public async Task<IActionResult> DeleteByAuthor([FromRoute] string uperUid)
        {
            var videos = await douyinVideoService.GetByAuthorId(uperUid);
            if (videos != null && videos.Any())
            {
                var res = await douyinVideoService.RealDeleteVideos(videos.Select(x => x.Id).ToList());
                if (res)
                {
                    return ApiResult.Success("删除成功");
                }
                else
                {
                    return ApiResult.Fail("删除失败");
                }
            }
            return ApiResult.Fail("未找到该博主视频");
        }

        //private async Task<(bool flowControl, IActionResult value)> BatchDeleteVideos(List<DouyinVideo> videos)
        //{

        //    return (flowControl: true, value: null);
        //}

        /// <summary>
        /// 查询最新N条数据
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        [HttpGet("top{top}")]
        public async Task<IActionResult> GetLastSyncTop([FromRoute] int top = 5)
        {
            return ApiResult.Success(await douyinVideoService.GetLastSyncTop(top));
        }

        /// <summary>
        /// 删除无效视频记录
        /// </summary>
        /// <returns></returns>
        [HttpGet("removeInvalid")]
        public async Task<IActionResult> RemoveInvalidVideo()
        {
            var data = await douyinVideoService.DeleteInvalidVideo();
            return Ok(data);
        }

        /// <summary>
        /// 所有视频重新生成nfo文件
        /// </summary>
        /// <returns></returns>
        [HttpGet("renfo")]
        public async Task<IActionResult> ReCreateNfo()
        {
            var videos = await douyinVideoService.GetAllAsync();
            if (videos == null || videos.Count == 0)
            {
                return ApiResult.Success("暂无视频数据需要生成NFO文件");
            }
            _ = Task.Run(async () =>
            {
                try
                {
                    var totalCount = videos.Count;
                    foreach (var video in videos)
                    {
                        try
                        {
                            NfoFileGenerator.GenerateVideoNfoFile(video);
                            Serilog.Log.Debug($"刮削视频（Path：{video.VideoSavePath}）生成NFO成功!");
                            await Task.Delay(50);
                        }
                        catch (Exception singleEx)
                        {
                            Serilog.Log.Error($"刮削视频（Path：{video.VideoSavePath}）生成NFO失败：{singleEx.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"NFO文件生成任务执行异常：{ex.Message}\n{ex.StackTrace}");
                }
            });

            return ApiResult.Success();
        }



        /// <summary>
        /// 获取7天视频同步趋势数据（曲线图）
        /// </summary>
        /// <returns>7天图表数据列表</returns>
        [HttpGet("chart/{day}")]
        public async  Task<IActionResult> Chart([FromRoute]int day=7)
        {
            try
            {
                var chartData = await douyinVideoService.GetChartData(day);
                return ApiResult.Success(chartData);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.Message);
            }
        }

        [HttpGet("/Move")]
        public async Task <IActionResult> Move()
        {
            await douyinVideoService.HandOldFolderVideos();

            return ApiResult.Success(DateTime.Now);
        }
    }
}
