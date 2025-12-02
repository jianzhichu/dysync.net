using dy.net.dto;
using dy.net.utils;
using NetTaste;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;

namespace dy.net.service
{
    public class DouyinHttpClientService
    {
        public static readonly string DouYinApi = "https://www.douyin.com/aweme/v1/web/aweme";
        // 随机数生成器（避免重复实例化，保证随机性）
        private readonly IHttpClientFactory _clientFactory;
        public DouyinHttpClientService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// 查询用户收藏的视频
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="count"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public async Task<DouyinVideoInfo> SyncCollectVideos(string cursor, string count, string cookie)
        {
            if (string.IsNullOrWhiteSpace(cursor))
            {
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            }

            if (string.IsNullOrWhiteSpace(count))
            {
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            }

            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));
            }

            try
            {
                using var httpClient = _clientFactory.CreateClient("dy_collect");
                if (httpClient.DefaultRequestHeaders.Contains("Cookie"))
                {
                    httpClient.DefaultRequestHeaders.Remove("Cookie");
                }
                httpClient.DefaultRequestHeaders.Add("Cookie", cookie);

                var dics = DouyinBaseParamDics.CollectParams;
                dics["cursor"]=cursor;
                dics["count"] = count;
                try
                {
                    var token = await TokenManager.GenRealMsTokenAsync();
                    dics["msToken"] = token;
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"获取mstoken失败{ex.Message}");
                }
            string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
            var endPoint = BogusManager.XbModel2Endpoint($"{DouYinApi}/listcollection", dics, UserAgent);
                var respose = await httpClient.PostAsync(endPoint, null);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<DouyinVideoInfo>(data);
                }
                else
                {
                    Serilog.Log.Error($"SyncCollectVideos fail: {respose.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"SyncCollectVideos error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 查询用户喜欢的视频
        /// </summary>
        /// <param name="count"></param>
        /// <param name="cursor"></param>
        /// <param name="secUserId"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<DouyinVideoInfo> SyncFavoriteVideos(string count,string cursor, string secUserId, string cookie)
        {
            if (string.IsNullOrWhiteSpace(cursor))
            {
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            }

            if (string.IsNullOrWhiteSpace(secUserId))
            {
                throw new ArgumentException($"“{nameof(secUserId)}”不能为 null 或空。", nameof(secUserId));
            }

            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));
            }

            try
            {
                using var httpClient = _clientFactory.CreateClient("dy_favorite");
                if (httpClient.DefaultRequestHeaders.Contains("Cookie"))
                {
                    httpClient.DefaultRequestHeaders.Remove("Cookie");
                }
                httpClient.DefaultRequestHeaders.Add("Cookie", cookie);

                var dics = DouyinBaseParamDics.FavoriteParams;
                {
                    // 添加动态参数
                    dics["max_cursor"] = cursor;
                    dics["sec_user_id"] = secUserId;
                    dics["count"] = count;
                }

                // 构建请求URL
                var queryString = new FormUrlEncodedContent(dics);
                string fullUrl = $"{DouYinApi}/favorite?{await queryString.ReadAsStringAsync()}";
                var respose = await httpClient.GetAsync(fullUrl);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<DouyinVideoInfo>(data);
                }
                else
                {
                    Serilog.Log.Error($"SyncFavoriteVideos fail: {respose.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"SyncFavoriteVideos error: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// 查询Up主作品
        /// </summary>
        /// <param name="count"></param>
        /// <param name="cursor"></param>
        /// <param name="secUserId"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<DouyinVideoInfo> SyncUpderPostVideos(string count, string cursor, string secUserId, string cookie)
        {
            if (string.IsNullOrWhiteSpace(cursor))
            {
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            }

            if (string.IsNullOrWhiteSpace(secUserId))
            {
                throw new ArgumentException($"“{nameof(secUserId)}”不能为 null 或空。", nameof(secUserId));
            }

            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));
            }

            try
            {
                using var httpClient = _clientFactory.CreateClient("dy_uper");
                if (httpClient.DefaultRequestHeaders.Contains("Cookie"))
                {
                    httpClient.DefaultRequestHeaders.Remove("Cookie");
                }
                httpClient.DefaultRequestHeaders.Add("Cookie", cookie);

                var parameters = DouyinBaseParamDics.InitializeDouyinPostParams();//修复关注的不下载图文视频
                {
                    // 添加动态参数
                    parameters["max_cursor"] = cursor;
                    parameters["sec_user_id"] = secUserId;
                    parameters["count"] = count;
                }

                // 构建请求URL
                var queryString = new FormUrlEncodedContent(parameters);
                string fullUrl = $"{DouYinApi}/post?{await queryString.ReadAsStringAsync()}";
                //var ablog = new ABogus();//计算X-Bogus
                //var a_bogus = ablog.GetValue(parameters);

                //fullUrl += $"&X-Bogus={a_bogus}";
                var respose = await httpClient.GetAsync(fullUrl);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<DouyinVideoInfo>(data);
                }
                else
                {
                    Serilog.Log.Error($"SyncUpderPostVideos fail: {respose.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"SyncUpderPostVideos error: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// 查询我的关注用户列表
        /// </summary>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <param name="secUserId"></param>
        /// <param name="cookie"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public async Task<DouyinFollowInfo> SyncMyFollows(string count,string offset,string secUserId,string cookie,Action<FollowErrorDto> callBack)
        {

            try
            {
                using var httpClient = _clientFactory.CreateClient("dy_follow");
                if (httpClient.DefaultRequestHeaders.Contains("Cookie"))
                {
                    httpClient.DefaultRequestHeaders.Remove("Cookie");
                }
                httpClient.DefaultRequestHeaders.Add("Cookie", cookie);
                var dics = DouyinBaseParamDics.MyFollowParams;
                {
                    // 添加动态参数
                    dics["sec_user_id"] = secUserId;
                    dics["count"] = count;
                    dics["offset"] = offset;
                }
                // 构建请求URL
                var queryString = new FormUrlEncodedContent(dics);
                string fullUrl = $"https://www.douyin.com/aweme/v1/web/user/following/list/?{await queryString.ReadAsStringAsync()}";
                var respose = await httpClient.GetAsync(fullUrl);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var res= JsonConvert.DeserializeObject<DouyinFollowInfo>(data);
                    if (res != null)
                    {
                        if (res.Followings == null)
                        {
                            var err = JsonConvert.DeserializeObject<FollowErrorDto>(data);
                         
                            if (err != null)
                            {
                                Serilog.Log.Error($"SyncMyFollows error: {err.StatusMsg}");
                                callBack(err);
                            }
                        }
                        else
                        {
                            callBack(new FollowErrorDto { StatusCode = 0 });
                        }
                    }
                    else
                    {
                        Serilog.Log.Error($"SyncMyFollows error: 反序列化结果为空,接口返回数据：{data}");
                        callBack(new FollowErrorDto { StatusCode = 8,StatusMsg="未知" });
                    }
                    return res;
                }
                else
                {
                    Serilog.Log.Error($"SyncMyFollows fail: {respose.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"SyncMyFollows error: {ex.Message}");
                return null;
            }
        }


        public async Task<bool> SyncReDown(string awemeId,string cookie)
        {
            return false;
            //有空再研究把.a_bogus算法有问题
        }

        //AI优化2
        /// <summary>
        /// 下载文件并保存到本地（支持重试机制，优化批量下载稳定性）
        /// </summary>
        /// <param name="videoUrl">文件地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="cookie">请求Cookie</param>
        /// <param name="httpclientName">HttpClient名称（默认"dy_down1"）</param>
        /// <param name="cancellationToken">取消令牌（用于终止任务）</param>
        /// <param name="streamTimeout">流读取超时时间（默认60秒）</param>
        /// <param name="maxRetryCount">最大重试次数（默认3次）</param>
        /// <param name="initialRetryDelay">初始重试延迟（默认1秒，指数退避）</param>
        /// <returns>是否下载成功</returns>
        public async Task<bool> DownloadAsync(
            string videoUrl,
            string savePath,
            string cookie,
            CancellationToken cancellationToken = default,
            TimeSpan? streamTimeout = null,
            int maxRetryCount = 3,
            TimeSpan? initialRetryDelay = null)
        {
            // 重试参数初始化
            int retryCount = 0;
            var retryDelay = initialRetryDelay ?? TimeSpan.FromSeconds(1); // 初始延迟1秒
            streamTimeout ??= TimeSpan.FromSeconds(60); // 流读取超时默认60秒

            while (true)
            {
                try
                {
                    // 执行单次下载逻辑（提取为内部方法，避免代码重复）
                    return await TryDownloadOnceAsync(
                        videoUrl, savePath, cookie, cancellationToken, streamTimeout.Value);
                }
                catch (Exception ex) when (IsRetryableException(ex) && retryCount < maxRetryCount)
                {
                    // 可重试异常且未达最大次数，等待后重试
                    retryCount++;
                    var delay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * Math.Pow(2, retryCount - 1)); // 指数退避（1s→2s→4s...）
                    Serilog.Log.Warning(ex, $"下载失败（第{retryCount}/{maxRetryCount}次重试）：{videoUrl}，将在{delay.TotalSeconds:F1}秒后重试");

                    try
                    {
                        // 等待重试延迟（响应取消令牌）
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // 等待期间被取消，直接返回失败
                        Serilog.Log.Information($"重试等待被取消：{videoUrl}");
                        return false;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // 主动取消，不重试
                    Serilog.Log.Information($"下载被取消：{videoUrl}");
                    return false;
                }
                catch (Exception ex)
                {
                    // 不可重试的异常（如参数错误、权限不足等），直接失败
                    Serilog.Log.Error(ex, $"下载失败（不可重试）：{videoUrl}");
                    CleanupIncompleteFile(savePath); // 清理不完整文件
                    return false;
                }
            }
        }

        /// <summary>
        /// 单次下载尝试（核心下载逻辑）
        /// </summary>
        private async Task<bool> TryDownloadOnceAsync(
            string videoUrl,
            string savePath,
            string cookie,
            CancellationToken cancellationToken,
            TimeSpan streamTimeout)
        {
            DateTime lastStreamActivity = DateTime.UtcNow; // 流活动时间戳

            // 确保目录存在
            var directory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 清理已存在的文件（避免占用）
            CleanupIncompleteFile(savePath);

            using (var httpClient = _clientFactory.CreateClient("dy_download"))
            {
                // 配置请求头
                httpClient.DefaultRequestHeaders.Remove("Cookie");
                httpClient.DefaultRequestHeaders.Add("Cookie", cookie);
                httpClient.Timeout = TimeSpan.FromMinutes(5); // 总请求超时

                // 发送请求
                using (var response = await httpClient.GetAsync(
                    videoUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode(); // 验证HTTP状态

                    long? totalBytes = response.Content.Headers.ContentLength;

                    // 读取流并写入文件
                    using (var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                    using (var fileStream = new FileStream(
                        savePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 8192,
                        options: FileOptions.Asynchronous | FileOptions.SequentialScan))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        long totalRead = 0;

                        while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                        {
                            // 检查流读取超时
                            if (DateTime.UtcNow - lastStreamActivity > streamTimeout)
                            {
                                throw new TimeoutException($"流读取超时（{streamTimeout.TotalSeconds}秒无数据）");
                            }

                            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                            totalRead += bytesRead;
                            lastStreamActivity = DateTime.UtcNow;

                            // 进度计算（可选）
                            if (totalBytes.HasValue)
                            {
                                double progress = (double)totalRead / totalBytes.Value * 100;
                                // 进度上报逻辑：OnProgressChanged(progress);
                            }
                        }

                        await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false); // 确保数据写入磁盘
                    }
                }
            }

            return true;
        }


        ///// <summary>下载网络文件到指定路径</summary>
        //public async Task DownloadFileAsync(string url, string savePath)
        //{
        //    if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
        //    if (string.IsNullOrEmpty(savePath)) throw new ArgumentNullException(nameof(savePath));

        //    // 创建目录（如果不存在）
        //    var directory = Path.GetDirectoryName(savePath)!;
        //    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        //    // 下载文件
        //    using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        //    response.EnsureSuccessStatusCode(); // 非2xx状态码抛出异常

        //    using var stream = await response.Content.ReadAsStreamAsync();
        //    using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        //    await stream.CopyToAsync(fileStream);
        //}



        /// <summary>
        /// 判断异常是否可重试
        /// </summary>
        private bool IsRetryableException(Exception ex)
        {
            // 可重试的异常类型：网络错误、超时、服务器临时错误等
            return ex is HttpRequestException // HTTP请求失败（如5xx错误、连接中断）
                || ex is TimeoutException // 超时（包括流超时和请求超时）
                || ex is IOException // IO错误（如临时磁盘问题）
                || (ex is AggregateException aggEx && aggEx.InnerExceptions.Any(IsRetryableException)); // 聚合异常中包含可重试项
        }

        /// <summary>
        /// 清理不完整的文件
        /// </summary>
        private static void CleanupIncompleteFile(string savePath)
        {
            if (File.Exists(savePath))
            {
                try
                {
                    File.Delete(savePath);
                }
                catch (IOException ex)
                {
                    Serilog.Log.Warning(ex, $"清理不完整文件失败：{savePath}（可能被占用）");
                }
            }
        }

    }
}
