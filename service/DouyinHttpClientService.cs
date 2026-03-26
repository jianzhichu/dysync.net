using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.utils;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace dy.net.service
{
    public class DouyinHttpClientService : IDisposable
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerSettings _jsonSettings=new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
        private bool _disposedValue;

        public DouyinHttpClientService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// 异步获取HTTP响应消息（优化版）
        /// </summary>
        private async Task<HttpResponseMessage> GetHttpResponseMessage(
            HttpMethod httpMethod,
            string requestUrl,
            Dictionary<string, string> requestParameters,
            string refererValue,
            string cookie)
        {
            string fullUrl = requestUrl;
            if (requestParameters != null && requestParameters.Count > 0)
            {
                fullUrl = QueryHelpers.AddQueryString(
                    requestUrl,
                    requestParameters.ToDictionary(
                        kv => kv.Key,
                        kv => new StringValues(kv.Value)
                    )
                );
            }

            using var requestMessage = new HttpRequestMessage(httpMethod, fullUrl);

            if (!string.IsNullOrEmpty(refererValue) && Uri.IsWellFormedUriString(refererValue, UriKind.Absolute))
            {
                requestMessage.Headers.Referrer = new Uri(refererValue);
            }

            if (!string.IsNullOrEmpty(cookie))
            {
                requestMessage.Headers.TryAddWithoutValidation("Cookie", cookie);
            }

            // 优化：移除using，由IHttpClientFactory管理生命周期
            var httpClient = _clientFactory.CreateClient(DouyinRequestParamManager.DY_HTTP_CLIENT);
            return await httpClient.SendAsync(requestMessage);
        }

        #region 收藏夹相关方法（保留原有逻辑，仅优化资源释放）
        public async Task<DouyinVideoInfoResponse> SyncCollectVideos(string cursor, string count, string cookie)
        {
            if (string.IsNullOrWhiteSpace(cursor))
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            if (string.IsNullOrWhiteSpace(count))
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            if (string.IsNullOrWhiteSpace(cookie))
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));

            try
            {
                var requestUrl = "/aweme/v1/web/aweme/listcollection";
                var refererValue = "https://www.douyin.com";

                var requestParameters = DouyinRequestParamManager.DouyinCollectParams;
                requestParameters["cursor"] = cursor;
                requestParameters["count"] = count;

                using var response = await GetHttpResponseMessage(HttpMethod.Post, requestUrl, requestParameters, refererValue, cookie);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);
                    var model = JsonSerializer.Create(_jsonSettings).Deserialize<DouyinVideoInfoResponse>(jsonReader);
                    if (model == null)
                        Log.Error($"SyncCollectVideos fail, data is null");
                    return model;
                }
                else
                {
                    Log.Error($"SyncCollectVideos fail: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SyncCollectVideos error: {ex.Message}", ex);
                return null;
            }
        }

        public async Task<DouyinCollectListResponse> SyncCollectFolderList(string cookie, string cursor)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                Log.Error("cookie为空，无法获取收藏夹列表 ");
                return null;
            }

            try
            {
                var requestUrl = "/aweme/v1/web/collects/list";
                var refererValue = "https://www.douyin.com/user/self?from_tab_name=main&showSubTab=favorite_folder&showTab=favorite_collection";

                var requestParameters = DouyinRequestParamManager.DouyinCollectListParams;
                requestParameters["count"] = "10";
                requestParameters["cursor"] = cursor;

                using var response = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);
                    var model = JsonSerializer.Create(_jsonSettings).Deserialize<DouyinCollectListResponse>(jsonReader);
                    if (model == null)
                        Log.Error($"SyncCollectFolderList fail, data is null");
                    return model;
                }
                else
                {
                    Log.Error("SyncCollectFolderList ,{StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error("SyncCollectFolderList ,{error}", ex);
                return null;
            }
        }

        public async Task<DouyinVideoInfoResponse> SyncCollectVideosByCollectId(string cursor, string count, string cookie, string collectsId)
        {
            if (string.IsNullOrWhiteSpace(collectsId))
                throw new ArgumentException($"“{nameof(collectsId)}”不能为 null 或空。", nameof(collectsId));
            if (string.IsNullOrWhiteSpace(cursor))
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            if (string.IsNullOrWhiteSpace(count))
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            if (string.IsNullOrWhiteSpace(cookie))
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));

            try
            {
                var requestUrl = "/aweme/v1/web/collects/video/list";
                var refererValue = "https://www.douyin.com/user/self?from_tab_name=main&showSubTab=favorite_folder&showTab=favorite_collection";

                var requestParameters = DouyinRequestParamManager.DouyinFolderCollectParams;
                requestParameters["cursor"] = cursor;
                requestParameters["count"] = "15";
                requestParameters["collects_id"] = collectsId;

                using var response = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);
                    var model = JsonSerializer.Create(_jsonSettings).Deserialize<DouyinVideoInfoResponse>(jsonReader);
                    if (model == null)
                        Log.Error($"SyncCollectVideosByCollectId fail: data is null");
                    return model;
                }
                else
                {
                    Log.Error($"SyncCollectVideosByCollectId fail: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SyncCollectVideosByCollectId error: {ex.Message}", ex);
                return null;
            }
        }
        #endregion

        #region 合集相关方法
        public async Task<DouyinMixListResponse> SyncMixList(string cookie, string cursor)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                Log.Error("cookie为空，无法获取收藏夹列表 ");
                return null;
            }

            try
            {
                var requestUrl = "/aweme/v1/web/mix/listcollection";
                var refererValue = "https://www.douyin.com/user/self?";

                var requestParameters = DouyinRequestParamManager.DouyinMixListParams;
                requestParameters["count"] = "10";
                requestParameters["cursor"] = cursor;

                using var response = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);
                    var model = JsonSerializer.Create(_jsonSettings).Deserialize<DouyinMixListResponse>(jsonReader);
                    if (model == null)
                        Log.Error($"SyncMixList fail, data is null");
                    return model;
                }
                else
                {
                    Log.Error($"SyncMixList : {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error("SyncMixList ,{error}", ex);
                return null;
            }
        }

        public async Task<DouyinVideoInfoResponse> SyncMixViedosByMixId(string cursor, string count, string cookie, string mixId)
        {
            if (string.IsNullOrWhiteSpace(mixId))
                throw new ArgumentException($"“{nameof(mixId)}”不能为 null 或空。", nameof(mixId));
            if (string.IsNullOrWhiteSpace(cursor))
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            if (string.IsNullOrWhiteSpace(count))
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            if (string.IsNullOrWhiteSpace(cookie))
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));

            try
            {
                var requestUrl = "/aweme/v1/web/mix/aweme";
                var refererValue = "https://www.douyin.com/user/self?";

                var requestParameters = DouyinRequestParamManager.DouyinMixVideoParams;
                requestParameters["cursor"] = cursor;
                requestParameters["count"] = "15";
                requestParameters["mix_id"] = mixId;

                using var response = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);
                    var model = JsonSerializer.Create(_jsonSettings).Deserialize<DouyinVideoInfoResponse>(jsonReader);
                    if (model == null)
                        Log.Error($"SyncMixViedosByMixId fail:data is null");
                    return model;
                }
                else
                {
                    Log.Error($"SyncMixViedosByMixId fail: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SyncMixViedosByMixId error: {ex.Message}", ex);
                return null;
            }
        }
        #endregion

        #region 短剧相关方法
        public async Task<DouyinSeriesListResponse> SyncSeriesList(string cookie, string cursor)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                Log.Error("cookie为空，无法获取收藏夹列表 ");
                return null;
            }

            try
            {
                var requestUrl = "/aweme/v1/web/series/collections";
                var refererValue = "https://www.douyin.com/user/self?";

                var requestParameters = DouyinRequestParamManager.DouyinSeriesListParams;
                requestParameters["count"] = "15";
                requestParameters["cursor"] = cursor;

                using var response = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);
                    var model = JsonSerializer.Create(_jsonSettings).Deserialize<DouyinSeriesListResponse>(jsonReader);
                    if (model == null)
                        Log.Error($"SyncShortList fail, data is null");
                    return model;
                }
                else
                {
                    Log.Error($"SyncShortList fail: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error("SyncShortList ,{error}", ex);
                return null;
            }
        }

        public async Task<DouyinVideoInfoResponse> SyncSeriesViedosByMSeriesId(string cursor, string count, string cookie, string seriesId)
        {
            if (string.IsNullOrWhiteSpace(seriesId))
                throw new ArgumentException($"“{nameof(seriesId)}”不能为 null 或空。", nameof(seriesId));
            if (string.IsNullOrWhiteSpace(cursor))
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            if (string.IsNullOrWhiteSpace(count))
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            if (string.IsNullOrWhiteSpace(cookie))
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));

            try
            {
                var requestUrl = "/aweme/v1/web/series/aweme";
                var refererValue = "https://www.douyin.com/user/self?";

                var requestParameters = DouyinRequestParamManager.DouyinSeriesVideosParams;
                requestParameters["cursor"] = cursor;
                requestParameters["count"] = count;
                requestParameters["series_id"] = seriesId;

                using var response = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);
                    var model = JsonSerializer.Create(_jsonSettings).Deserialize<DouyinVideoInfoResponse>(jsonReader);
                    if (model == null)
                        Log.Error($"SyncSeriesViedosByMSeriesId fail:data is null");
                    return model;
                }
                else
                {
                    Log.Error($"SyncSeriesViedosByMSeriesId fail: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SyncSeriesViedosByMSeriesId error: {ex.Message}", ex);
                return null;
            }
        }
        #endregion

        #region 喜欢/博主/关注相关方法
        public async Task<DouyinVideoInfoResponse> SyncFavoriteVideos(string count, string cursor, string secUserId, string cookie)
        {
            if (string.IsNullOrWhiteSpace(cursor))
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            if (string.IsNullOrWhiteSpace(count))
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            if (string.IsNullOrWhiteSpace(secUserId))
                throw new ArgumentException($"“{nameof(secUserId)}”不能为 null 或空。", nameof(secUserId));
            if (string.IsNullOrWhiteSpace(cookie))
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));

            try
            {
                var requestUrl = "/aweme/v1/web/aweme/favorite";
                var refererValue = "https://www.douyin.com/user/self?showTab=like";

                var requestParameters = DouyinRequestParamManager.DouyinFavoriteParams;
                requestParameters["max_cursor"] = cursor;
                requestParameters["sec_user_id"] = secUserId;
                requestParameters["count"] = count;

                using var response = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);
                    var model = JsonSerializer.Create(_jsonSettings).Deserialize<DouyinVideoInfoResponse>(jsonReader);
                    if (model == null)
                        Log.Error($"SyncFavoriteVideos fail, data is null");
                    return model;
                }
                else
                {
                    Log.Error($"SyncFavoriteVideos fail: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SyncFavoriteVideos error: {ex.Message}", ex);
                return null;
            }
        }

        public async Task<DouyinVideoInfoResponse> SyncUpderPostVideos(string count, string cursor, string secUserId, string cookie)
        {
            if (string.IsNullOrWhiteSpace(cursor))
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            if (string.IsNullOrWhiteSpace(count))
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            if (string.IsNullOrWhiteSpace(secUserId))
                throw new ArgumentException($"“{nameof(secUserId)}”不能为 null 或空。", nameof(secUserId));
            if (string.IsNullOrWhiteSpace(cookie))
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));

            try
            {
                var requestUrl = "/aweme/v1/web/aweme/post";
                var refererValue = "https://www.douyin.com/user/";

                var requestParameters = DouyinRequestParamManager.DouyinUpderPostParams;
                requestParameters["max_cursor"] = cursor;
                requestParameters["sec_user_id"] = secUserId;
                requestParameters["count"] = count;

                using var response = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);
                    var model = JsonSerializer.Create(_jsonSettings).Deserialize<DouyinVideoInfoResponse>(jsonReader);
                    if (model == null)
                        Log.Error($"SyncUpderPostVideos fail, data is null");
                    return model;
                }
                else
                {
                    Log.Error($"SyncUpderPostVideos StatusCode fail: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SyncUpderPostVideos error: {ex.Message}", ex);
                return null;
            }
        }

        public async Task<DouyinFollowInfoResponse> SyncMyFollows(string count, string offset, string secUserId, string cookie)
        {
            if (string.IsNullOrWhiteSpace(offset))
                throw new ArgumentException($"“{nameof(offset)}”不能为 null 或空。", nameof(offset));
            if (string.IsNullOrWhiteSpace(count))
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            if (string.IsNullOrWhiteSpace(secUserId))
                throw new ArgumentException($"“{nameof(secUserId)}”不能为 null 或空。", nameof(secUserId));
            if (string.IsNullOrWhiteSpace(cookie))
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));

            try
            {
                var requestUrl = "/aweme/v1/web/user/following/list";
                var refererValue = "https://www.douyin.com/user/self?showTab=like";

                var requestParameters = DouyinRequestParamManager.DouyinMyFollowParams;
                requestParameters["sec_user_id"] = secUserId;
                requestParameters["count"] = count;
                requestParameters["offset"] = offset;

                using var response = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);
                    using var jsonReader = new JsonTextReader(reader);
                    var model = JsonSerializer.Create(_jsonSettings).Deserialize<DouyinFollowInfoResponse>(jsonReader);
                    return model;
                }
                else
                {
                    Log.Error($"SyncMyFollows fail: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SyncMyFollows error: {ex.Message}", ex);
                // 优化：抛出前确保资源释放，使用包装异常保留原始堆栈
                throw new InvalidOperationException("获取关注列表失败", ex);
            }
        }
        #endregion

        #region Cookie检查
        public async Task<bool> CheckCookie(DouyinCookie douyinCookie)
        {
            if (douyinCookie == null || string.IsNullOrWhiteSpace(douyinCookie.Cookies))
                return false;

            try
            {
                if (!string.IsNullOrWhiteSpace(douyinCookie.SecUserId))
                {
                    var res = await SyncMyFollows("20", "0", douyinCookie.SecUserId, douyinCookie.Cookies);
                    return res != null && res.StatusCode == 0;
                }
                else
                {
                    var res = await SyncCollectVideos("0", "18", douyinCookie.Cookies);
                    return res != null && res.StatusCode == 0;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "检查Cookie有效性失败");
                return false;
            }
        }
        #endregion

        #region 下载相关（优化内存管理）
        public async Task<(bool Success, string ActualSavePath)> DownloadAsync(
           string videoUrl,
           string savePath,
           string cookie,
           List<string> otherUrls = null,
           CancellationToken cancellationToken = default,
           TimeSpan? streamTimeout = null,
           int maxRetryCount = 3,
           TimeSpan? initialRetryDelay = null)
        {
            // 优化1：隔离外部列表，避免引用泄漏
            var retryUrls = new List<string> { videoUrl };
            if (otherUrls != null && otherUrls.Any())
            {
                retryUrls.AddRange(otherUrls);
                maxRetryCount = Math.Min(maxRetryCount, retryUrls.Count); // 防止溢出
            }

            int retryCount = 0;
            var retryDelay = initialRetryDelay ?? TimeSpan.FromSeconds(1);
            streamTimeout ??= TimeSpan.FromSeconds(60);

            while (retryCount < maxRetryCount)
            {
                try
                {
                    string currentUrl = retryUrls[retryCount];
                    return await TryDownloadOnceAsync(currentUrl, savePath, cookie, cancellationToken, streamTimeout.Value);
                }
                catch (Exception ex) when (IsRetryableException(ex) && retryCount < maxRetryCount - 1)
                {
                    retryCount++;
                    var delay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * Math.Pow(2, retryCount - 1));
                    Log.Warning(ex, $"下载失败（第{retryCount}/{maxRetryCount}次重试）：{videoUrl}，将在{delay.TotalSeconds:F1}秒后重试");

                    try
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        Log.Information($"重试等待被取消：{videoUrl}");
                        return (false, savePath);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    Log.Information($"下载被取消：{videoUrl}");
                    return (false, savePath);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"下载失败（不可重试）：{videoUrl}");
                    CleanupIncompleteFile(savePath);
                    return (false, savePath);
                }
            }

            Log.Error($"下载失败：已达最大重试次数 {maxRetryCount}，URL={videoUrl}");
            return (false, savePath);
        }

        private async Task<(bool Success, string ActualSavePath)> TryDownloadOnceAsync(
            string videoUrl,
            string savePath,
            string cookie,
            CancellationToken cancellationToken,
            TimeSpan streamTimeout)
        {
            DateTime lastStreamActivity = DateTime.UtcNow;
            string actualSavePath = savePath;
            string detectedExtension = string.Empty;

            // 确保目录存在
            var directory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 清理已存在的文件
            CleanupIncompleteFile(savePath);

            // 优化2：使用using包裹下载专用HttpClient（工厂创建的Client仍可using，释放内部handler）
            using var httpClient = _clientFactory.CreateClient(DouyinRequestParamManager.DY_HTTP_CLIENT_DOWN);
            try
            {
                // 配置请求头
                httpClient.DefaultRequestHeaders.Remove("Cookie"); // 先移除再添加，避免重复
                httpClient.DefaultRequestHeaders.Add("Cookie", cookie);
                httpClient.Timeout = TimeSpan.FromMinutes(5);

                // 优化3：使用HttpCompletionOption.ResponseHeadersRead，不缓存整个响应
                using var response = await httpClient.GetAsync(
                    videoUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;

                // 检测文件类型
                var ext = Path.GetExtension(savePath);
                if (ext == ".mp3")
                {
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        if (contentType.Contains("audio/mp4") || contentType.Contains("audio/m4a"))
                            detectedExtension = "m4a";
                        else if (contentType.Contains("audio/mpeg") || contentType.Contains("audio/mp3"))
                            detectedExtension = "mp3";
                        else if (contentType.Contains("video/mp4"))
                            detectedExtension = "mp4";
                    }
                }

                // 修正保存路径
                if (!string.IsNullOrEmpty(detectedExtension))
                {
                    actualSavePath = Path.ChangeExtension(actualSavePath, detectedExtension.Trim('.'));
                    CleanupIncompleteFile(actualSavePath);
                }

                // 优化4：流式写入文件，且使用ArrayPool复用缓冲区（减少大对象分配）
                using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var fileStream = new FileStream(
                    actualSavePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 8192,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

                var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(8192); // 复用缓冲区
                try
                {
                    int bytesRead;
                    long totalRead = 0;

                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        if (DateTime.UtcNow - lastStreamActivity > streamTimeout)
                            throw new TimeoutException($"流读取超时（{streamTimeout.TotalSeconds}秒无数据）");

                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                        totalRead += bytesRead;
                        lastStreamActivity = DateTime.UtcNow;
                    }

                    await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    System.Buffers.ArrayPool<byte>.Shared.Return(buffer); // 归还缓冲区
                }

                return (true, actualSavePath);
            }
            catch
            {
                CleanupIncompleteFile(actualSavePath);
                throw;
            }
            finally
            {
                //httpClient.DefaultRequestHeaders.Clear(); // 清空请求头，帮助GC
            }
        }

        private bool IsRetryableException(Exception ex)
        {
            return ex is HttpRequestException
                || ex is TimeoutException
                || ex is IOException
                || (ex is AggregateException aggEx && aggEx.InnerExceptions.Any(IsRetryableException));
        }

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
                    Log.Error(ex, $"清理无效文件失败：{savePath}（可能被占用）");
                }
            }
        }
        #endregion

        #region IDisposable 实现（优化版）
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // 释放托管资源：仅清理本类创建的托管资源（此处无）
                }

                // 释放非托管资源：本类无非托管资源
                // 优化：移除强制GC调用，让GC自动管理

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}