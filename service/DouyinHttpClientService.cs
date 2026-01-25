using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.utils;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace dy.net.service
{
    public class DouyinHttpClientService
    {


        private readonly IHttpClientFactory _clientFactory;
        public DouyinHttpClientService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }


        /// <summary>
        /// 异步获取HTTP响应消息（优化版）
        /// </summary>
        /// <param name="requestParameters">URL请求参数键值对</param>
        /// <param name="httpMethod">HTTP请求方法（GET/POST等）</param>
        /// <param name="requestUrl">基础请求URL</param>
        /// <param name="refererValue">Referer请求头值（可为null/空）</param>
        /// <param name="cookie">Cookie请求头值（可为null/空）</param>
        /// <returns>HTTP响应消息</returns>
        private async Task<HttpResponseMessage> GetHttpResponseMessage(
            HttpMethod httpMethod,
            string requestUrl,
            Dictionary<string, string> requestParameters,
            string refererValue,
            string cookie)
        {
            string fullUrl = "{requestUrl}";
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
                // 此处兼容直接添加Cookie头，增加格式校验
                requestMessage.Headers.TryAddWithoutValidation("Cookie", cookie);
            }

            using var httpClient = _clientFactory.CreateClient(DouyinRequestParamManager.DY_HTTP_CLIENT);
            return await httpClient.SendAsync(requestMessage);
        }


        #region 收藏夹（默认，不分自定义文件夹）

        /// <summary>
        /// 查询用户默认收藏的视频
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="count"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public async Task<DouyinVideoInfoResponse> SyncCollectVideos(string cursor, string count, string cookie)
        {
            #region 检查参数
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
            #endregion

            try
            {
                var requestUrl = "/aweme/v1/web/aweme/listcollection";
                var refererValue = "https://www.douyin.com";

                var requestParameters = DouyinRequestParamManager.DouyinCollectParams;
                {
                    requestParameters["cursor"] = cursor;
                    requestParameters["count"] = count;
                }

                var respose = await GetHttpResponseMessage(HttpMethod.Post, requestUrl, requestParameters, refererValue, cookie);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<DouyinVideoInfoResponse>(data);
                    if (model == null)
                        Serilog.Log.Error($"SyncCollectVideos fail: {data}");
                    return model;
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

        #endregion

        #region 收藏夹（自定义收藏文件名称）

        /// <summary>
        /// 获取自定义收藏夹列表
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public async Task<DouyinCollectListResponse> SyncCollectFolderList(string cookie, string cursor)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                Serilog.Log.Error("cookie为空，无法获取收藏夹列表 ");
                return null;
            }

            try
            {
                var requestUrl = "/aweme/v1/web/collects/list";
                var refererValue = "https://www.douyin.com/user/self?from_tab_name=main&showSubTab=favorite_folder&showTab=favorite_collection";

                var requestParameters = DouyinRequestParamManager.DouyinCollectListParams;
                {
                    // 添加动态参数
                    requestParameters["count"] = "100"; //一次性获取100个收藏文件夹（应该够用）
                    requestParameters["cursor"] = cursor; //页码
                }

                var respose = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<DouyinCollectListResponse>(data);
                    if (model == null)
                        Serilog.Log.Error($"SyncCollectFolderList fail: {data}");
                    return model;
                }
                else
                {
                    Serilog.Log.Error("SyncCollectFolderList ,{StatusCode}", respose.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("SyncCollectFolderList ,{errro}", ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// 根据收藏夹Id查询收藏夹的视频
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="count"></param>
        /// <param name="cookie"></param>
        /// <param name="collectsId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<DouyinVideoInfoResponse> SyncCollectVideosByCollectId(string cursor, string count, string cookie, string collectsId)
        {
            #region 检查参数
            if (string.IsNullOrWhiteSpace(collectsId))
            {
                throw new ArgumentException($"“{nameof(collectsId)}”不能为 null 或空。", nameof(collectsId));
            }
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
            #endregion

            try
            {

                var requestUrl = "/aweme/v1/web/collects/video/list";
                var refererValue = "https://www.douyin.com/user/self?from_tab_name=main&showSubTab=favorite_folder&showTab=favorite_collection";

                var requestParameters = DouyinRequestParamManager.DouyinFolderCollectParams;
                {
                    count = "15";
                    requestParameters["cursor"] = cursor;
                    requestParameters["count"] = count;
                    requestParameters["collects_id"] = collectsId;
                }

                var respose = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);

                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<DouyinVideoInfoResponse>(data);
                    if (model == null)
                        Serilog.Log.Error($"SyncCollectVideosByCollectId fail: {data}");
                    return model;
                }
                else
                {
                    Serilog.Log.Error($"SyncCollectVideosByCollectId fail: {respose.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"SyncCollectVideosByCollectId error: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region 收藏夹（合集）

        /// <summary>
        /// 获取收藏合集列表
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public async Task<DouyinMixListResponse> SyncMixList(string cookie, string cursor)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                Serilog.Log.Error("cookie为空，无法获取收藏夹列表 ");
                return null;
            }

            try
            {
                var requestUrl = "/aweme/v1/web/mix/listcollection";
                var refererValue = "https://www.douyin.com/user/self?";

                var requestParameters = DouyinRequestParamManager.DouyinMixListParams;
                {
                    // 添加动态参数
                    requestParameters["count"] = "100";
                    requestParameters["cursor"] = cursor; //页码
                }

                var respose = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<DouyinMixListResponse>(data);
                    if (model == null)
                        Serilog.Log.Error($"SyncMixList fail: {data}");
                    return model;
                }
                else
                {
                    Serilog.Log.Error($"SyncMixList : {respose.StatusCode}");
                    return null;
                }

            }
            catch (Exception ex)
            {
                Serilog.Log.Error("SyncMixList ,{errro}", ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// 根据合集ID查询合集视频列表
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="count"></param>
        /// <param name="cookie"></param>
        /// <param name="mix"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<DouyinVideoInfoResponse> SyncMixViedosByMixId(string cursor, string count, string cookie, string mixId)
        {
            #region 检查参数
            if (string.IsNullOrWhiteSpace(mixId))
            {
                throw new ArgumentException($"“{nameof(mixId)}”不能为 null 或空。", nameof(mixId));
            }
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
            #endregion

            try
            {

                var requestUrl = "/aweme/v1/web/mix/aweme";
                var refererValue = "https://www.douyin.com/user/self?";

                var requestParameters = DouyinRequestParamManager.DouyinMixVideoParams;
                {
                    requestParameters["cursor"] = cursor;
                    requestParameters["count"] = "15";
                    requestParameters["mix_id"] = mixId;
                }

                var respose = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);

                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<DouyinVideoInfoResponse>(data);
                    if (model == null)
                        Serilog.Log.Error($"SyncMixViedosByMixId fail: {data}");
                    return model;
                }
                else
                {
                    Serilog.Log.Error($"SyncMixViedosByMixId fail: {respose.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"SyncMixViedosByMixId error: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region 收藏夹（短剧）

        /// <summary>
        /// 获取短剧列表
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public async Task<DouyinSeriesListResponse> SyncSeriesList(string cookie, string cursor)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                Serilog.Log.Error("cookie为空，无法获取收藏夹列表 ");
                return null;
            }

            try
            {
                var requestUrl = "/aweme/v1/web/series/collections";
                var refererValue = "https://www.douyin.com/user/self?";

                var requestParameters = DouyinRequestParamManager.DouyinSeriesListParams;
                {
                    // 添加动态参数
                    requestParameters["count"] = "15";//固定15，多了直接返回参数不合法
                    requestParameters["cursor"] = cursor; //页码
                }

                var respose = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<DouyinSeriesListResponse>(data);
                    if (model == null)
                        Serilog.Log.Error($"SyncShortList fail: {data}");
                    return model;
                }
                else
                {
                    Serilog.Log.Error($"SyncShortList fail: {respose.StatusCode}");
                    return null;
                }

            }
            catch (Exception ex)
            {
                Serilog.Log.Error("SyncShortList ,{errro}", ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="count"></param>
        /// <param name="cookie"></param>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<DouyinVideoInfoResponse> SyncSeriesViedosByMSeriesId(string cursor, string count, string cookie, string seriesId)
        {
            #region 检查参数
            if (string.IsNullOrWhiteSpace(seriesId))
            {
                throw new ArgumentException($"“{nameof(seriesId)}”不能为 null 或空。", nameof(seriesId));
            }
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
            #endregion

            try
            {

                var requestUrl = "/aweme/v1/web/series/aweme";
                var refererValue = "https://www.douyin.com/user/self?";

                var requestParameters = DouyinRequestParamManager.DouyinSeriesVideosParams;
                {
                    requestParameters["cursor"] = cursor;
                    requestParameters["count"] = count;
                    requestParameters["series_id"] = seriesId;
                }

                var respose = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);

                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<DouyinVideoInfoResponse>(data);
                    if (model == null)
                        Serilog.Log.Error($"SyncSeriesViedosByMSeriesId fail: {data}");
                    return model;
                }
                else
                {
                    Serilog.Log.Error($"SyncSeriesViedosByMSeriesId fail: {respose.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"SyncSeriesViedosByMSeriesId error: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region 喜欢的（点赞视频同步）

        /// <summary>
        /// 查询用户喜欢的视频
        /// </summary>
        /// <param name="count"></param>
        /// <param name="cursor"></param>
        /// <param name="secUserId"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<DouyinVideoInfoResponse> SyncFavoriteVideos(string count, string cursor, string secUserId, string cookie)
        {

            #region 检查参数
            if (string.IsNullOrWhiteSpace(cursor))
            {
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            }
            if (string.IsNullOrWhiteSpace(count))
            {
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            }

            if (string.IsNullOrWhiteSpace(secUserId))
            {
                throw new ArgumentException($"“{nameof(secUserId)}”不能为 null 或空。", nameof(secUserId));
            }

            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));
            }
            #endregion

            try
            {
                var requestUrl = "/aweme/v1/web/aweme/favorite";
                var refererValue = "https://www.douyin.com/user/self?showTab=like";

                var requestParameters = DouyinRequestParamManager.DouyinFavoriteParams;
                {
                    // 添加动态参数
                    requestParameters["max_cursor"] = cursor;
                    requestParameters["sec_user_id"] = secUserId;
                    requestParameters["count"] = count;
                }
                var respose = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);

                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<DouyinVideoInfoResponse>(data);
                    if (model == null)
                        Serilog.Log.Error($"SyncFavoriteVideos fail: {data}");
                    return model;
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

        #endregion

        #region 博主视频（关注的博主作品同步）

        /// <summary>
        /// 查询Up主作品
        /// </summary>
        /// <param name="count"></param>
        /// <param name="cursor"></param>
        /// <param name="secUserId"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<DouyinVideoInfoResponse> SyncUpderPostVideos(string count, string cursor, string secUserId, string cookie)
        {
            #region 检查参数
            if (string.IsNullOrWhiteSpace(cursor))
            {
                throw new ArgumentException($"“{nameof(cursor)}”不能为 null 或空。", nameof(cursor));
            }
            if (string.IsNullOrWhiteSpace(count))
            {
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            }

            if (string.IsNullOrWhiteSpace(secUserId))
            {
                throw new ArgumentException($"“{nameof(secUserId)}”不能为 null 或空。", nameof(secUserId));
            }

            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));
            }
            #endregion

            try
            {
                var requestUrl = "/aweme/v1/web/aweme/post";
                var refererValue = "https://www.douyin.com/user/";

                var requestParameters = DouyinRequestParamManager.DouyinUpderPostParams;//修复关注的不下载图文视频
                {
                    // 添加动态参数
                    requestParameters["max_cursor"] = cursor;
                    requestParameters["sec_user_id"] = secUserId;
                    requestParameters["count"] = count;
                }
                var respose = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);

                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<DouyinVideoInfoResponse>(data);
                    if (model == null)
                        Serilog.Log.Error($"SyncUpderPostVideos fail: {data}");
                    return model;
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

        #endregion

        #region 关注列表（同步关注列表）

        /// <summary>
        /// 查询我的关注用户列表
        /// </summary>
        /// <param name="count"></param>
        /// <param name="offset"></param>
        /// <param name="secUserId"></param>
        /// <param name="cookie"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public async Task<DouyinFollowInfoResponse> SyncMyFollows(string count, string offset, string secUserId, string cookie, Action<FollowErrorDto> callBack)
        {

            #region 检查参数
            if (string.IsNullOrWhiteSpace(offset))
            {
                throw new ArgumentException($"“{nameof(offset)}”不能为 null 或空。", nameof(offset));
            }
            if (string.IsNullOrWhiteSpace(count))
            {
                throw new ArgumentException($"“{nameof(count)}”不能为 null 或空。", nameof(count));
            }

            if (string.IsNullOrWhiteSpace(secUserId))
            {
                throw new ArgumentException($"“{nameof(secUserId)}”不能为 null 或空。", nameof(secUserId));
            }

            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentException($"“{nameof(cookie)}”不能为 null 或空。", nameof(cookie));
            }
            #endregion

            try
            {
                var requestUrl = "/aweme/v1/web/user/following/list";
                var refererValue = "https://www.douyin.com/user/self?showTab=like";

                var requestParameters = DouyinRequestParamManager.DouyinMyFollowParams;
                {
                    // 添加动态参数
                    requestParameters["sec_user_id"] = secUserId;
                    requestParameters["count"] = count;
                    requestParameters["offset"] = offset;
                }

                var respose = await GetHttpResponseMessage(HttpMethod.Get, requestUrl, requestParameters, refererValue, cookie);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<DouyinFollowInfoResponse>(data);
                    if (res != null)
                    {
                        if (res.Followings == null)
                        {
                            var err = JsonConvert.DeserializeObject<FollowErrorDto>(data);

                            if (err != null)
                            {
                                Serilog.Log.Error($"SyncMyFollows error: {err.StatusMsg}");
                                callBack?.Invoke(err);
                            }
                            Serilog.Log.Error($"SyncMyFollows error: 关注列表为空 :{data}");
                        }
                        else
                        {
                            callBack?.Invoke(new FollowErrorDto { StatusCode = 0 });
                        }
                    }
                    else
                    {
                        Serilog.Log.Error($"SyncMyFollows error: 反序列化结果为空,接口返回数据：{data}");
                        callBack?.Invoke(new FollowErrorDto { StatusCode = 8, StatusMsg = "未知" });
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
                throw;
            }
        }

        #endregion

        #region 检查cookie是否有效

        /// <summary>
        /// 检查cookie是否有效
        /// </summary>
        /// <param name="douyinCookie"></param>
        /// <returns></returns>
        public async Task<bool> CheckCookie(DouyinCookie douyinCookie)
        {
            if (douyinCookie == null || string.IsNullOrWhiteSpace(douyinCookie.Cookies)) return false;

            if (!string.IsNullOrWhiteSpace(douyinCookie.SecUserId))
            {
                var res = await SyncMyFollows("1", "1", douyinCookie.SecUserId, douyinCookie.Cookies, null);
                return res != null && res.status_code == 0 && res.Followings != null && res.Followings.Any();
            }
            else
            {
                var res = await SyncCollectVideos("0", "1", douyinCookie.Cookies);
                return res != null && res.AwemeList != null && res.AwemeList.Any();
            }
        }

        #endregion

        #region 资源下载相关

        /// <summary>
        /// 下载文件并保存到本地
        /// </summary>
        /// <param name="videoUrl">文件地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="cookie">请求Cookie</param>
        /// <param name="otherUrls">其他下载地址</param>
        /// <param name="cancellationToken">取消令牌（用于终止任务）</param>
        /// <param name="streamTimeout">流读取超时时间（默认60秒）</param>
        /// <param name="maxRetryCount">最大重试次数（默认3次）</param>
        /// <param name="initialRetryDelay">初始重试延迟（默认1秒，指数退避）</param>
        /// <returns>是否下载成功</returns>
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
            // 重试参数初始化
            int retryCount = 0;
            var retryDelay = initialRetryDelay ?? TimeSpan.FromSeconds(1); // 初始延迟1秒
            streamTimeout ??= TimeSpan.FromSeconds(60); // 流读取超时默认60秒
            if (otherUrls != null)
            {
                maxRetryCount = otherUrls.Count;
            }
            while (true)
            {
                try
                {
                    if (otherUrls != null && retryCount > 0)
                    {
                        return await TryDownloadOnceAsync(
                          otherUrls[retryCount], savePath, cookie, cancellationToken, streamTimeout.Value);
                    }
                    else
                    {
                        return await TryDownloadOnceAsync(
                            videoUrl, savePath, cookie, cancellationToken, streamTimeout.Value);
                    }


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
                        return (false, savePath);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // 主动取消，不重试
                    Serilog.Log.Information($"下载被取消：{videoUrl}");
                    return (false, savePath);
                }
                catch (Exception ex)
                {
                    // 不可重试的异常（如参数错误、权限不足等），直接失败
                    Serilog.Log.Error(ex, $"下载失败（不可重试）：{videoUrl}");
                    CleanupIncompleteFile(savePath); // 清理不完整文件
                    return (false, savePath);
                }
            }
        }

        /// <summary>
        /// 单次下载尝试（核心下载逻辑）
        /// </summary>
       private async Task<(bool Success, string ActualSavePath)> TryDownloadOnceAsync(
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
            //传入的文件后缀名
            string ext = Path.GetExtension(savePath);
            string actualSavePath = savePath;
            string detectedExtension = string.Empty;


            using (var httpClient = _clientFactory.CreateClient(DouyinRequestParamManager.DY_HTTP_CLIENT_DOWN))
            {
                // 配置请求头
                if (httpClient.DefaultRequestHeaders.Contains("Cookie"))
                    httpClient.DefaultRequestHeaders.Remove("Cookie");
                httpClient.DefaultRequestHeaders.Add("Cookie", cookie);
                httpClient.Timeout = TimeSpan.FromMinutes(5); // 总请求超时

                // 发送请求
                using var response = await httpClient.GetAsync(
                    videoUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode(); // 验证HTTP状态

                long? totalBytes = response.Content.Headers.ContentLength;

                // 读取响应流
                using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

                if (ext == ".mp3")
                {
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        if (contentType.Contains("audio/mp4") || contentType.Contains("audio/m4a"))
                        {
                            detectedExtension = "m4a";
                        }
                        else if (contentType.Contains("audio/mpeg") || contentType.Contains("audio/mp3"))
                        {
                            detectedExtension = "mp3";
                        }
                        else if (contentType.Contains("video/mp4"))
                        {
                            detectedExtension = "mp4";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(detectedExtension))
                {
                    string extension = Path.GetExtension(actualSavePath);
                    if (string.IsNullOrEmpty(extension))
                    {
                        actualSavePath += detectedExtension;
                    }
                    else
                    {
                        actualSavePath = Path.ChangeExtension(actualSavePath, detectedExtension.Trim('.'));
                        savePath = actualSavePath;
                    }
                    CleanupIncompleteFile(actualSavePath);
                }

                using var fileStream = new FileStream(
                    actualSavePath, // 使用修正后的保存路径
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 8192,
                    options: FileOptions.Asynchronous | FileOptions.SequentialScan);
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

            // 返回成功状态和实际的保存路径
            return (Success: true, ActualSavePath: actualSavePath);
        }

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
                    Serilog.Log.Error(ex, $"清理无效文件失败：{savePath}（可能被占用）");
                }
            }
        }

        #endregion
    }


}
