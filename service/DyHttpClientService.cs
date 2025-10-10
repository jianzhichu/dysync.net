using dy.net.dto;
using dy.net.utils;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;

namespace dy.net.service
{
    public class DyHttpClientService
    {

        private static readonly string CollectApi = "https://www.douyin.com/aweme/v1/web/aweme/listcollection/";
        private static readonly string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
        // 随机数生成器（避免重复实例化，保证随机性）
        private readonly IHttpClientFactory _clientFactory;
        public DyHttpClientService(IHttpClientFactory clientFactory)
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
        public async Task<CollectVideoInfo> SyncCollectVideos(string cursor, string count, string cookie)
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
                using var httpClient = _clientFactory.CreateClient("douyin");
                if (httpClient.DefaultRequestHeaders.Contains("Cookie"))
                {
                    httpClient.DefaultRequestHeaders.Remove("Cookie");
                }
                httpClient.DefaultRequestHeaders.Add("Cookie", cookie);

                var dics = CreateUserCollectHeader();
                dics.Add("cursor", cursor);
                dics.Add("count", count);
                try
                {
                    var token = await TokenManager.GenRealMsTokenAsync();
                    dics.Add("msToken", token);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"获取mstoken失败{ex.Message}");
                }
                var endPoint = BogusManager.XbModel2Endpoint(CollectApi, dics, UserAgent);
                var respose = await httpClient.PostAsync(endPoint, null);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CollectVideoInfo>(data);
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
        /// <param name="cursor"></param>
        /// <param name="secUserId"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<CollectVideoInfo> SyncFavoriteVideos(string cursor, string secUserId, string cookie)
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
                using var httpClient = _clientFactory.CreateClient("douyinfav");
                if (httpClient.DefaultRequestHeaders.Contains("Cookie"))
                {
                    httpClient.DefaultRequestHeaders.Remove("Cookie");
                }
                httpClient.DefaultRequestHeaders.Add("Cookie", cookie);

                var dics = CreateUserFavoriteParams();
                dics.Add("max_cursor", cursor);
                dics.Add("sec_user_id", secUserId);
                // 构建请求URL
                string baseUrl = "https://www.douyin.com/aweme/v1/web/aweme/favorite/";
                var queryString = new FormUrlEncodedContent(dics);
                string fullUrl = $"{baseUrl}?{await queryString.ReadAsStringAsync()}";
                var respose = await httpClient.GetAsync(fullUrl);
                if (respose.IsSuccessStatusCode)
                {
                    var data = await respose.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CollectVideoInfo>(data);
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
        /// 下载文件并保存到本地
        /// </summary>
        /// <param name="videoUrl">文件地址</param>
        /// <param name="savePath">保存路径</param>
        public async Task<bool> DownloadAsync(string videoUrl, string savePath, string cookie)
        {
            try
            {
                // 防止文件被占用，先删除已存在的文件
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
                // 创建HTTP客户端（设置超时时间和请求头）
                using (var httpClient = _clientFactory.CreateClient("douyin"))
                {
                    if (httpClient.DefaultRequestHeaders.Contains("Cookie"))
                    {
                        httpClient.DefaultRequestHeaders.Remove("Cookie");
                    }
                    httpClient.DefaultRequestHeaders.Add("Cookie", cookie);
                    httpClient.Timeout = TimeSpan.FromMinutes(5); // 设置5分钟超时

                    // 获取视频流
                    using (var response = await httpClient.GetAsync(videoUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode(); // 确保请求成功

                        // 获取文件总大小（用于进度显示）
                        long? totalBytes = response.Content.Headers.ContentLength;

                        // 读取流并写入文件
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(savePath, FileMode.CreateNew))
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            long totalRead = 0;

                            // 循环读取并写入
                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalRead += bytesRead;

                                // 显示下载进度
                                if (totalBytes.HasValue)
                                {
                                    double progress = (double)totalRead / totalBytes.Value * 100;
                                    //Console.Write($"\r下载进度：{progress:F2}% ({totalRead}/{totalBytes.Value} bytes)");
                                }
                            }
                            //Console.WriteLine(); // 进度显示结束后换行
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"DownloadVideoAsync fail: {ex.Message}");
                Serilog.Log.Error($"DownloadVideoAsync fail: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// 清理文件名中的非法字符
        /// </summary>
        static string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            // 限制文件名长度
            return fileName.Length > 100 ? fileName.Substring(0, 100) : fileName;
        }


        // 创建包含默认值的字典
        Dictionary<string, string> CreateUserCollectHeader()
        {
            return new Dictionary<string, string>
        {
            {"device_platform", "webapp"},
            {"aid", "6383"},
            {"channel", "channel_pc_web"},
            {"pc_client_type", "1"},
            {"version_code", "290100"},
            {"version_name", "29.1.0"},
            {"cookie_enabled", "true"},
            {"screen_width", "1920"},
            {"screen_height", "1080"},
            {"browser_language", "zh-CN"},
            {"browser_platform", "Win32"},
            {"browser_name", "Chrome"},
            {"browser_version", "130.0.0.0"},
            {"browser_online", "true"},
            {"engine_name", "Blink"},
            {"engine_version", "130.0.0.0"},
            {"os_name", "Windows"},
            {"os_version", "10"},
            {"cpu_core_num", "12"},
            {"device_memory", "8"},
            {"platform", "PC"},
            {"downlink", "10"},
            {"effective_type", "4g"},
            {"from_user_page", "1"},
            {"locate_query", "false"},
            {"need_time_list", "1"},
            {"pc_libra_divert", "Windows"},
            {"publish_video_strategy_type", "2"},
            {"round_trip_time", "0"},
            {"show_live_replay_strategy", "1"},
            {"time_list_query", "0"},
            {"whale_cut_token", ""},
            {"update_version_code", "170400"}
        };
        }
        Dictionary<string, string> CreateUserFavoriteParams()
        {

         var parameters = new Dictionary<string, string>
        {
            {"device_platform", "webapp"},
            {"aid", "6383"},
            {"channel", "channel_pc_web"},
            {"min_cursor", "0"},
            {"whale_cut_token", ""},
            {"cut_version", "1"},
            {"count", "18"},
            {"publish_video_strategy_type", "2"},
            {"update_version_code", "170400"},
            {"pc_client_type", "1"},
            {"pc_libra_divert", "Windows"},
            {"support_h265", "1"},
            {"support_dash", "1"},
            {"cpu_core_num", "20"},
            {"version_code", "170400"},
            {"version_name", "17.4.0"},
            {"cookie_enabled", "true"},
            {"screen_width", "1536"},
            {"screen_height", "960"},
            {"browser_language", "zh-CN"},
            {"browser_platform", "Win32"},
            {"browser_name", "Chrome"},
            {"browser_version", "140.0.0.0"},
            {"browser_online", "true"},
            {"engine_name", "Blink"},
            {"engine_version", "140.0.0.0"},
            {"os_name", "Windows"},
            {"os_version", "10"},
            {"device_memory", "8"},
            {"platform", "PC"},
            {"downlink", "10"},
            {"effective_type", "4g"},
            {"round_trip_time", "0"},
            {"webid", "7516440221375268388"}
        };
            return parameters;
        }

    }
}
