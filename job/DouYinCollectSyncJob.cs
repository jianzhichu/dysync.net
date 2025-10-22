using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
using Quartz;

namespace dy.net.job
{
    [DisallowConcurrentExecution]
    public class DouYinCollectSyncJob : IJob
    {
        private readonly DyCookieService _dyCookieService;

        private readonly DyHttpClientService _douyinService;

        private readonly DyCollectVideoService _douyinVideoService;
        private readonly CommonService commonService;
        private readonly Random _random = new Random();
        private string count = "18"; // 每页请求的视频数量，默认18

        public DouYinCollectSyncJob(DyCookieService dyCookieService, DyHttpClientService dyHttpClientService, DyCollectVideoService dyCollectVideoService, CommonService commonService)
        {
            _dyCookieService = dyCookieService;
            _douyinService = dyHttpClientService;
            _douyinVideoService = dyCollectVideoService;
            this.commonService = commonService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var config = commonService.GetConfig();
            if (config == null)
            {
                Serilog.Log.Debug("collect-请先在设置中初始化配置，再执行同步任务");
                return;
            }
            if (config.BatchCount > 0)
            {
                count = config.BatchCount.ToString();
            }
            Serilog.Log.Debug($"collect-当前设置的每次扫描行数为:{count}");

            var cookies = await _dyCookieService.GetAllCookies();
            if (cookies == null || !cookies.Any())
            {
                Serilog.Log.Debug("collect-无可用Cookie，任务终止");
                return;
            }
            else
            {
                Serilog.Log.Debug($"collect-当前有{cookies.Count}个cookie开启了同步,即将开始同步");
                //return;
            }

            //每天凌晨1点到1点半之间执行一次重置同步状态-全部cookie
            UpdateCookieSyncedToZero();

            foreach (var cookie in cookies)
            {
              
                Serilog.Log.Debug($"collect-开始同步 Cookie-[{cookie.UserName}]收藏视频");
                if (string.IsNullOrWhiteSpace(cookie.Cookies) || cookie.Cookies.Length < 1000)
                {
                    Serilog.Log.Debug($"collect-Cookie-[{cookie.UserName}]无效，跳过");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(cookie.SavePath))
                {
                    Serilog.Log.Debug($"collect-Cookie-[{cookie.UserName}]未设置保存路径，跳过");
                    continue;
                }
                try
                {
                    int syncCount = 0;// 记录本次Cookie同步的视频数量

                    int index = 0;
                    bool hasMore = true;

                    string cursor = "0";

                    while (hasMore)
                    {
                        var data = await _douyinService.SyncCollectVideos(cursor, count, cookie.Cookies);
                        hasMore = data != null && data.HasMore == 1 && cookie.CollHasSyncd == 0;
                        if (cookie.CollHasSyncd == 1)
                        {
                            Serilog.Log.Debug($"collect-Cookie[{cookie.UserName}]已完整同步过，后续只获取最新一页数据");
                        }
                        //Serilog.Log.Debug($"还有数据需要同步吗？{(hasMore ? "YES" : "NO")}");
                        if (data == null)
                        {
                            Serilog.Log.Debug($"collect-Cookie[{cookie.UserName}]获取收藏数据失败，请检查一下Cookie");
                            break;
                        }
                        cursor = data != null && !string.IsNullOrWhiteSpace(data.Cursor) ? data.Cursor : "0";


                        if (data.AwemeList == null || !data.AwemeList.Any())
                        {
                            break;
                        }

                        List<DyCollectVideo> videos = new List<DyCollectVideo>();
                        foreach (var item in data.AwemeList)
                        {
                            if (item == null)
                                continue;
                            if (item.Video == null)
                                continue;
                            if (item.Video.BitRate == null)
                                continue;
                            var v = item.Video.BitRate.FirstOrDefault();
                            var tags = item.VideoTags;
                            if (v == null) continue;

                            var videoUrl = v.PlayAddr.UrlList != null && v.PlayAddr.UrlList.Any() ? v.PlayAddr.UrlList[0] : null;
                            if (string.IsNullOrWhiteSpace(videoUrl)) continue;

                            var tag1 = tags.FirstOrDefault(x => x.Level == 1)?.TagName;
                            var tag2 = tags.FirstOrDefault(x => x.Level == 2)?.TagName;
                            var tag3 = tags.FirstOrDefault(x => x.Level == 3)?.TagName;
                            string saveFolder = CreateSaveFolder(cookie, item, tag1, tag2);

                         
                            var fileName = $"{item.AwemeId}.{v.Format}"; // 用ID做文件名，避免特殊字符
                            var savePath = Path.Combine(saveFolder, fileName);

                            if (!File.Exists(savePath))
                            {
                                Serilog.Log.Debug($"collect-视频[{SanitizePath(item.Desc)}]开始下载");
                                await Task.Delay(_random.Next(1, 4) * 1000);
                                var downVideo = await _douyinService.DownloadAsync(videoUrl, savePath, cookie.Cookies);

                                if (downVideo)
                                {
                                    Serilog.Log.Debug($"collect-视频[{SanitizePath(item.Desc)}]下载{(downVideo ? "成功" : "失败")}");
                                }
                                else
                                {
                                    Serilog.Log.Error($"collect-视频[{SanitizePath(item.Desc)}]下载{(downVideo ? "成功" : "失败")}");

                                }
                                if (downVideo)
                                {
                                    await DownVideoCover(item, saveFolder, cookie.Cookies);

                                    // 用AuthorId做文件名，避免昵称特殊字符
                                    var avatarImgName = $"{item.Author.Uid}.jpg";
                                    var avatarSavePath = Path.Combine(cookie.SavePath, "author", avatarImgName);
                                    await DownAuthorAvatar(cookie.SavePath, item, avatarSavePath, cookie.Cookies);
                                    var avatarUrl = item.Author?.AvatarLarger?.UrlList != null && item.Author.AvatarLarger.UrlList.Any()
                                       ? item.Author.AvatarLarger.UrlList[0] : null;
                                    // 备用头像链接
                                    if (string.IsNullOrWhiteSpace(avatarUrl))
                                    {
                                        avatarUrl = item.Author?.AvatarThumb?.UrlList != null && item.Author.AvatarThumb.UrlList.Any()
                                            ? item.Author.AvatarThumb.UrlList[0] : null;
                                    }



                                    // 构造视频数据
                                    DyCollectVideo video = new()
                                    {
                                        ViedoType = "2",
                                        AwemeId = item.AwemeId,
                                        Author = item.Author?.Nickname,
                                        AuthorId = item.Author?.Uid,
                                        AuthorAvatar = avatarSavePath,
                                        AuthorAvatarUrl = avatarUrl,
                                        CreateTime = DateTimeUtil.Convert10BitTimestamp(item.CreateTime),
                                        VideoTitle = item.Desc,
                                        Id = IdGener.GetLong().ToString(),
                                        Resolution = $"{v.PlayAddr.Width}×{v.PlayAddr.Height}",
                                        FileSize = v.PlayAddr.DataSize,
                                        FileHash = v.PlayAddr.FileHash,
                                        Tag1 = tag1,
                                        Tag2 = tag2,
                                        Tag3 = tag3,
                                        VideoUrl = videoUrl,
                                        VideoCoverUrl = item.Video.Cover.UrlList != null && item.Video.Cover.UrlList.Any()
                                            ? item.Video.Cover.UrlList[0] : null,
                                        VideoSavePath = savePath,
                                        VideoCoverSavePath = Path.Combine(saveFolder, "poster.jpg"),
                                        SyncTime = DateTime.Now,
                                        DyUserId = data.Uid,
                                        CookieId = cookie.Id
                                    };
                                    videos.Add(video);

                                    var nfoPath = Path.Combine(saveFolder, $"{item.AwemeId}.nfo");
                                    NfoFileGenerator.GenerateNfoFile(new VideoNfo
                                    {
                                        Author = video.Author,
                                        Poster = Path.Combine(saveFolder, "poster.jpg"),
                                        Title = video.VideoTitle,
                                        Thumbnail = Path.Combine(saveFolder, "fanart.jpg"),
                                        ReleaseDate = video.CreateTime,
                                        Genres = new List<string> { tag1, tag2, tag3 }.Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                                    }, nfoPath);

                                }
                            }
                            else
                            {
                                //Serilog.Log.Debug($"视频[{item.AwemeId}]已存在，跳过下载");
                            }
                        }
                        index++;
                        // 批量保存到数据库（减少数据库操作频率）
                        if (videos.Any())
                        {
                            //Serilog.Log.Debug($"处理Cookie[{cookie.UserName}]的第{index}页数据，共{videos.Count}条");
                            try
                            {
                                await _douyinVideoService.batchInsert(videos);
                                syncCount += videos.Count;
                            }
                            catch (Exception ex)
                            {
                                Serilog.Log.Error($"collect-批量保存视频到数据库失败：{ex.Message}", ex);

                                // 收集所有需要删除的目录（去重）
                                foreach (var video in videos)
                                {
                                    if (!string.IsNullOrWhiteSpace(video.VideoSavePath) && Directory.Exists(video.VideoSavePath))
                                    {
                                        var deletePath = Path.GetDirectoryName(video.VideoSavePath);
                                        Directory.Delete(deletePath, recursive: true);
                                        //Serilog.Log.Debug($"已删除失败视频目录：{deletePath}");
                                    }
                                }

                                Serilog.Log.Debug("collect-因为数据库没有保存成功，本次下载的视频目录已尝试删除，将继续处理下一页数据");
                            }
                        }
                        else
                        {
                            //Serilog.Log.Debug($"没有查询到新的视频");
                        }
                        await Task.Delay(_random.Next(5, 10) * 1000);
                    }
                    if (syncCount > 0)
                    {
                        Serilog.Log.Debug($"collect-Cookie-[{cookie.UserName}],本次共同步成功{syncCount}条视频");
                        // 更新同步状态为已同步
                        cookie.CollHasSyncd = 1;
                        await _dyCookieService.UpdateAsync(cookie);
                    }
                    else
                    {
                        Serilog.Log.Debug($"collect-Cookie-[{cookie.UserName}],本次没有查询到新的视频");
                    }

                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"collect-处理Cookie[{cookie.Id}]时出错：{ex.Message}");
                    Serilog.Log.Error($"collect-处理Cookie[{cookie.Id}]时出错22：{ex.StackTrace}");
                }


            }
        }

        /// <summary>
        /// 下载视频封面
        /// </summary>
        /// <param name="item"></param>
        /// <param name="saveFolder"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        private async Task DownVideoCover(Aweme item, string saveFolder, string cookie)
        {
            var coverUrl = item.Video.Cover.UrlList != null && item.Video.Cover.UrlList.Any()
                ? item.Video.Cover.UrlList[0] : null;
            if (string.IsNullOrWhiteSpace(coverUrl)) return;

            var coverImgName = "poster.jpg";
            var coverSavePath = Path.Combine(saveFolder, coverImgName);

            if (!File.Exists(coverSavePath))
            {
                // 封面下载前随机延迟
                var downRes = await _douyinService.DownloadAsync(coverUrl, coverSavePath, cookie);
                if (downRes)
                {
                    var copyPath = Path.Combine(saveFolder, "fanart.jpg");
                    File.Copy(coverSavePath, copyPath, true);
                }
            }
        }

        /// <summary>
        /// 下载作者头像
        /// </summary>
        /// <param name="mainPath"></param>
        /// <param name="item"></param>
        /// <param name="avatarSavePath"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        private async Task DownAuthorAvatar(string mainPath, Aweme item, string avatarSavePath, string cookie)
        {
            if (item.Author == null) return;

            var avatarUrl = item.Author.AvatarLarger?.UrlList != null && item.Author.AvatarLarger.UrlList.Any()
                ? item.Author.AvatarLarger.UrlList[0] : null;
            if (string.IsNullOrWhiteSpace(avatarUrl)) return;
            var path = Path.Combine(mainPath, "author");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (!File.Exists(avatarSavePath))
            {
                // 头像下载前随机延迟
                await _douyinService.DownloadAsync(avatarUrl, avatarSavePath, cookie);
            }
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="tag1"></param>
        /// <param name="tag2"></param>
        /// <returns></returns>
        private static string CreateSaveFolder(DyUserCookies cookie, Aweme item, string? tag1, string? tag2)
        {
            // 路径中避免特殊字符，用ID替代描述
            var safeTag1 = string.IsNullOrWhiteSpace(tag1) ? "other" : SanitizePath(tag1);
            List<string> pathParts = new List<string> { cookie.SavePath, safeTag1 };
            var saveFolder = Path.Combine(pathParts[0], string.Join("-", pathParts.Skip(1)), SanitizePath(item.Desc) + "@" + item.AwemeId);

            // 创建文件夹（提前创建，避免下载时才操作）
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
            return saveFolder;
            //if (string.IsNullOrWhiteSpace(tag2))
            //    return Path.Combine(pathParts[0], string.Join("-", pathParts.Skip(1)), SanitizePath(item.Desc) + "@" + item.AwemeId);
            //else
            //    return Path.Combine(pathParts[0], string.Join("-", pathParts.Skip(1)), tag2 + "@" + item.AwemeId);
        }

        // 清理路径中的特殊字符（避免创建文件夹失败）
        private static string SanitizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "其他";
            }
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                path = path.Replace(c, '_');
            }
            if (path.Length > 50)
            {
                path = path.Substring(0, 50);
            }
            return path.Trim();
        }

        /// <summary>
        /// 重置Cookie的同步状态为0（每天凌晨1点到1点半之间执行一次）
        /// </summary>
        /// <returns></returns>
        private void UpdateCookieSyncedToZero()
        {
            var now = DateTime.Now;
            //如果当时间为凌晨1点到1点半之间，则重置为0
            if (now.Hour == 1 && now.Minute < 30)
            {
                commonService.UpdateAllCookieSyncedToZero();
            }
        }
    }
}
