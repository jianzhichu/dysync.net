using ClockSnowFlake;
using dy.image;
using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
using Quartz;
using Quartz.Util;

namespace dy.net.job
{
    [DisallowConcurrentExecution]
    public abstract class DouyinBaseSyncJob : IJob
    {
        protected readonly DouyinCookieService _dyCookieService;
        protected readonly DouyinHttpClientService _douyinService;
        protected readonly DouyinVideoService _douyinVideoService;
        protected readonly DouyinCommonService _commonService;
        protected readonly Random _random = new Random();
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IWebHostEnvironment _environment;
        protected string count = "18"; // 每页请求的视频数量
        protected abstract string JobType { get; }


        private bool _downImageVideo;

        /// <summary>
        /// 清除保存失败的数据
        /// </summary>
        /// <param name="dyCookieService"></param>
        /// <param name="dyHttpClientService"></param>
        /// <param name="dyCollectVideoService"></param>
        /// <param name="commonService"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="webHostEnvironment"></param>
        protected DouyinBaseSyncJob(
            DouyinCookieService dyCookieService,
            DouyinHttpClientService dyHttpClientService,
            DouyinVideoService dyCollectVideoService,
            DouyinCommonService commonService, IServiceProvider serviceProvider, IWebHostEnvironment webHostEnvironment)
        {
            _dyCookieService = dyCookieService;
            _douyinService = dyHttpClientService;
            _douyinVideoService = dyCollectVideoService;
            _commonService = commonService;
            _serviceProvider = serviceProvider;
            _environment = webHostEnvironment;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var config = _commonService.GetConfig();
            if (config == null)
            {
                Serilog.Log.Debug($"{JobType}-请先在设置中初始化配置，再执行同步任务");
                return;
            }
            if (config.BatchCount > 0)
                count = config.BatchCount.ToString();

            // 读取环境变量覆盖配置中是否下载图片视频配置
            InitializeDownImageVideoSetting(config);

            await BeforeProcessCookies();

            var cookies = await GetValidCookies();
            if (cookies == null || !cookies.Any())
            {
                Serilog.Log.Debug($"{JobType}-无可用Cookie，任务终止");
                return;
            }

            Serilog.Log.Debug($"{JobType}-当前有{cookies.Count}个cookie开启了同步，即将开始同步");

            foreach (var cookie in cookies)
            {
                await ProcessSyncUserCookie(cookie);
            }
        }
        /// <summary>
        /// 检查并初始化下载图片视频的设置
        /// </summary>
        /// <param name="config"></param>
        private void InitializeDownImageVideoSetting(AppConfig config)
        {
            var downImageVideoConfig = Appsettings.Get("DOWN_IMGVIDEO");
            if (!string.IsNullOrWhiteSpace(downImageVideoConfig))
            {
                downImageVideoConfig = downImageVideoConfig.ToLower();
                _downImageVideo = config.DownImageVideo && (downImageVideoConfig == "1" || downImageVideoConfig == "y" || downImageVideoConfig == "t" || downImageVideoConfig == "true");
            }
        }

        /// <summary>
        /// 检查并处理Cookie
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        protected async Task ProcessSyncUserCookie(DouyinUserCookie cookie)
        {
            try
            {
                if (!IsCookieValid(cookie))
                {
                    Serilog.Log.Debug($"{JobType}-Cookie[{cookie.UserName}]无效，跳过");
                    return;
                }

                Serilog.Log.Debug($"{JobType}-开始同步 Cookie-[{cookie.UserName}]");

                int syncCount = 0;
                string cursor = "0";
                bool hasMore = true;

                while (hasMore)
                {
                    var data = await FetchVideoData(cookie, cursor);
                    if (data == null)
                    {
                        Serilog.Log.Debug($"{JobType}-Cookie[{cookie.UserName}]获取数据失败");
                        break;
                    }

                    hasMore = ShouldContinueSync(cookie, data);
                    cursor = GetNextCursor(data);

                    if (data.AwemeList == null || !data.AwemeList.Any())
                        break;

                    var videos = await ProcessVideoList(cookie, data);
                    syncCount += await SaveVideos(videos);

                    await Task.Delay(_random.Next(5, 10) * 1000);
                }

                await HandleSyncCompletion(cookie, syncCount);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"{JobType}-处理Cookie[{cookie.Id}]时出错");
            }
        }

        /// <summary>
        ///  处理接口返回的视频数据
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected async Task<List<DouyinVideo>> ProcessVideoList(DouyinUserCookie cookie, DouyinVideoInfo data)
        {
            var videos = new List<DouyinVideo>();
            foreach (var item in data.AwemeList)
            {
                var video = await ProcessSingleVideo(cookie, item, data);
                if (video != null)
                    videos.Add(video);

                //如果配置了下载图片视频，则处理图片集并合成视频
                if (_downImageVideo)
                {
                  var  mergevideo = await ProcessImageSetAndMergeToVideo(cookie, item, data);
                    if (mergevideo != null)
                        videos.Add(mergevideo);
                }
            }
            return videos;
        }

        /// <summary>
        /// 处理单个视频数据
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected async Task<DouyinVideo> ProcessSingleVideo(DouyinUserCookie cookie, Aweme item, DouyinVideoInfo data)
        {
            if (!IsAwemeValid(item)) return null;

            var v = item.Video.BitRate.FirstOrDefault();
            if (v == null) return null;

            var videoUrl = v.PlayAddr.UrlList?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(videoUrl)) return null;

            var (tag1, tag2, tag3) = GetVideoTags(item);
            var saveFolder = CreateSaveFolder(cookie, item, tag1, tag2);
            var fileName = GetVideoFileName(cookie, item, v);
            var savePath = Path.Combine(saveFolder, fileName);

            if (File.Exists(savePath)) return null;

            Serilog.Log.Debug($"{JobType}-视频[{TikTokFileNameHelper.SanitizePath(item.Desc)}]开始下载");
            await Task.Delay(_random.Next(1, 4) * 1000);
            if (!await _douyinService.DownloadAsync(videoUrl, savePath, cookie.Cookies))
            {
                Serilog.Log.Error($"{JobType}-视频[{TikTokFileNameHelper.SanitizePath(item.Desc)}]下载失败");
                return null;
            }

            await DownVideoCover(item, saveFolder, cookie.Cookies);
            var (avatarSavePath, avatarUrl) = await DownAuthorAvatar(cookie, item);
            await GenerateNfoFile(saveFolder, item, tag1, tag2, tag3, avatarSavePath, avatarUrl);
            return CreateVideoEntity(cookie, item, v, savePath, saveFolder, tag1, tag2, tag3, avatarSavePath, avatarUrl, data);
        }

        /// <summary>
        /// 创建视频实体
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="bitRate"></param>
        /// <param name="savePath"></param>
        /// <param name="saveFolder"></param>
        /// <param name="tag1"></param>
        /// <param name="tag2"></param>
        /// <param name="tag3"></param>
        /// <param name="avatarSavePath"></param>
        /// <param name="avatarUrl"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected DouyinVideo CreateVideoEntity(
            DouyinUserCookie cookie, Aweme item, VideoBitRate bitRate, string savePath, string saveFolder,
            string tag1, string tag2, string tag3, string avatarSavePath, string avatarUrl, DouyinVideoInfo data)
        {
            var diffs = GetVideoEntityDifferences(cookie, item);

            return new DouyinVideo
            {
                ViedoType = diffs.VideoType.GetHashCode().ToString(),
                AwemeId = item.AwemeId,
                Author = item.Author?.Nickname,
                AuthorId = item.Author?.Uid,
                AuthorAvatar = avatarSavePath,
                AuthorAvatarUrl = avatarUrl,
                CreateTime = DateTimeUtil.Convert10BitTimestamp(item.CreateTime),
                VideoTitle = item.Desc,
                VideoTitleSimplify = diffs.VideoTitleSimplify,
                Id = IdGener.GetLong().ToString(),
                Resolution = $"{bitRate.PlayAddr.Width}×{bitRate.PlayAddr.Height}",
                FileSize = bitRate.PlayAddr.DataSize,
                FileHash = bitRate.PlayAddr.FileHash,
                Tag1 = tag1,
                Tag2 = tag2,
                Tag3 = tag3,
                VideoUrl = bitRate.PlayAddr.UrlList?.FirstOrDefault(),
                VideoCoverUrl = item.Video.Cover.UrlList?.FirstOrDefault(),
                VideoSavePath = savePath,
                VideoCoverSavePath = Path.Combine(saveFolder, "poster.jpg"),
                SyncTime = DateTime.Now,
                DyUserId = item.AuthorUserId == 0 ? item.Author?.Uid : item.AuthorUserId.ToString(),
                CookieId = cookie.Id
            };
        }

        /// <summary>
        /// 保存视频信息到数据库
        /// </summary>
        /// <param name="videos"></param>
        /// <returns></returns>
        protected async Task<int> SaveVideos(List<DouyinVideo> videos)
        {
            if (!videos.Any()) return 0;
            try
            {
                await _douyinVideoService.batchInsert(videos);
                return videos.Count;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"{JobType}-批量保存视频到数据库失败");
                await CleanupFailedVideos(videos);
                return 0;
            }
        }
        /// <summary>
        /// 清理保存失败的视频文件
        /// </summary>
        /// <param name="videos"></param>
        /// <returns></returns>
        protected async Task CleanupFailedVideos(List<DouyinVideo> videos)
        {
            foreach (var video in videos)
            {
                if (!string.IsNullOrWhiteSpace(video.VideoSavePath) && Directory.Exists(Path.GetDirectoryName(video.VideoSavePath)))
                {
                    Directory.Delete(Path.GetDirectoryName(video.VideoSavePath), recursive: true);
                }
            }
            Serilog.Log.Debug($"{JobType}-因数据库保存失败，已清理本次下载的视频目录");
        }

        /// <summary>
        /// 下载视频封面
        /// </summary>
        /// <param name="item"></param>
        /// <param name="saveFolder"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        protected async Task DownVideoCover(Aweme item, string saveFolder, string cookie)
        {
            var coverUrl = item.Video.Cover.UrlList?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(coverUrl)) return;

            var coverSavePath = Path.Combine(saveFolder, "poster.jpg");
            if (File.Exists(coverSavePath)) return;

            if (await _douyinService.DownloadAsync(coverUrl, coverSavePath, cookie))
            {
                File.Copy(coverSavePath, Path.Combine(saveFolder, "fanart.jpg"), true);
            }
        }

        /// <summary>
        /// 下载图片合成的视频封面
        /// </summary>
        /// <param name="coverUrl"></param>
        /// <param name="saveFolder"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        private async Task DownVideoCover(string coverUrl, string saveFolder, string cookie)
        {
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
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected async Task<(string savePath, string url)> DownAuthorAvatar(DouyinUserCookie cookie, Aweme item)
        {
            if (item.Author == null) return (null, null);
            var avatarUrl = item.Author.AvatarLarger?.UrlList?.FirstOrDefault() ?? item.Author.AvatarThumb?.UrlList?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(avatarUrl)) return (null, null);

            var avatarSavePath = Path.Combine(GetAuthorAvatarBasePath(cookie), $"{item.Author.Uid}.jpg");
            var avatarDir = Path.GetDirectoryName(avatarSavePath);
            if (!Directory.Exists(avatarDir)) Directory.CreateDirectory(avatarDir);
            if (!File.Exists(avatarSavePath))
            {
                await _douyinService.DownloadAsync(avatarUrl, avatarSavePath, cookie.Cookies);
            }
            return (avatarSavePath, avatarUrl);
        }


        /// <summary>
        /// 处理图片集并合成为视频
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected async Task<DouyinVideo> ProcessImageSetAndMergeToVideo(DouyinUserCookie cookie, Aweme item, DouyinVideoInfo data)
        {
            try
            {
                List<string> imageUrls = item.Images?
                .Where(img => img.UrlList != null && img.UrlList.Any())
                .Select(img => img.UrlList.FirstOrDefault())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToList();

                if (imageUrls == null || !imageUrls.Any())
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(cookie.ImgSavePath))
                {
                    Serilog.Log.Error($"{JobType}-图片视频同步-没有配置图片存储路径");
                    return null;
                }
                var imageService = _serviceProvider.GetService<ImageMergeToVideoService>();
                if (imageService == null)
                {
                    Serilog.Log.Error($"{JobType} -图片视频同步-无法创建 ImageMergeToVideoService。");
                    return null;
                }
            

                var fileNamefolder = Path.Combine(cookie.ImgSavePath, TikTokFileNameHelper.GenerateFileName(item.Desc, item.AwemeId));
                if (!Directory.Exists(fileNamefolder)) Directory.CreateDirectory(fileNamefolder);
                var savePath = Path.Combine(fileNamefolder, $"{item.AwemeId}.mp4");

                if (File.Exists(savePath)) return null;
                var mp3Url = item.Music?.PlayUrl?.UrlList?.FirstOrDefault();

                var reqParams = new MediaMergeRequest
                {
                    ImageDurationPerSecond = 3,
                    OutputFormat = "mp4",
                    VideoFps = 30,
                    AudioUrls = string.IsNullOrWhiteSpace(mp3Url) ? new List<string>() : new List<string> { mp3Url },
                    ImageUrls = imageUrls,
                    VideoWidth = 1080,
                    VideoHeight = 1920,
                };

                var mergeResult = await imageService.MergeToVideo(AppContext.BaseDirectory, reqParams, savePath,fileNamefolder);
                if (!mergeResult)
                {
                    Serilog.Log.Error($"{JobType}-图片视频同步-视频[{TikTokFileNameHelper.SanitizePath(item.Desc)}]合成失败");
                    return null;
                }
                if (!File.Exists(savePath) || new FileInfo(savePath).Length <= 0)
                {
                    Serilog.Log.Error($"{JobType}-图片视频同步-视频[{TikTokFileNameHelper.SanitizePath(item.Desc)}]合成失败");
                    if (Directory.Exists(fileNamefolder))
                    {
                        File.Delete(savePath);
                        Directory.Delete(fileNamefolder, true);
                        Serilog.Log.Error($"{JobType}-图片视频同步-已删除合成失败的视频文件和目录");
                    }
                    return null;
                }

                // 复用基类的方法
                await DownVideoCover(imageUrls.FirstOrDefault(), fileNamefolder, cookie.Cookies);
                var (avatarSavePath, avatarUrl) = await DownAuthorAvatar(cookie, item);

                // 为合成的视频创建一个“虚拟”的BitRate对象，以便复用CreateVideoEntity
                var virtualBitRate = new VideoBitRate
                {
                    PlayAddr = new PlayAddr
                    {
                        Width = reqParams.VideoWidth,
                        Height = reqParams.VideoHeight,
                        DataSize = new FileInfo(savePath).Length
                    }
                };

                var (tag1, tag2, tag3) = GetVideoTags(item);
                await GenerateNfoFile(fileNamefolder, item, tag1, tag2, tag3, avatarSavePath, avatarUrl);

                var videoEntity = CreateVideoEntity(
                    cookie, item, virtualBitRate, savePath, fileNamefolder,
                    tag1, tag2, tag3, avatarSavePath, avatarUrl, data);

                // 特殊处理合成视频的字段
                videoEntity.FileHash = string.Empty;
                videoEntity.VideoUrl = "/"; // 合成视频没有原始URL
                videoEntity.ViedoType= VideoTypeEnum.ImageVideo.ToString();
                return videoEntity;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"{JobType}-图片视频同步-处理图片集并合成视频时出错");
                return null;
            }

        }

        /// <summary>
        /// 生成NFO文件
        /// </summary>
        protected async Task GenerateNfoFile(string saveFolder, Aweme item, string tag1, string tag2, string tag3,
            string avatarSavePath, string avatarUrl)
        {
            var nfoPath = Path.Combine(saveFolder, $"{item.AwemeId}.nfo");
            NfoFileGenerator.GenerateNfoFile(new DouyinVideoNfo
            {
                Actors = new List<Actor>
                {
                    new() {
                        Name = item.Author?.Nickname,
                        Role = "主演",
                        Thumb = avatarSavePath
                    }
                },
                Author = item.Author?.Nickname,
                Poster = Path.Combine(saveFolder, "poster.jpg"),
                Title = item.Desc,
                Thumbnail = Path.Combine(saveFolder, "fanart.jpg"),
                ReleaseDate = DateTimeUtil.Convert10BitTimestamp(item.CreateTime),
                Genres = new List<string> { tag1, tag2, tag3 }.Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
            }, nfoPath);
        }

        /// <summary>
        /// 检查视频数据是否有效
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected bool IsAwemeValid(Aweme item) => item != null && item.Video != null && item.Video.BitRate != null;
        /// <summary>
        /// 获取视频标签信息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected (string tag1, string tag2, string tag3) GetVideoTags(Aweme item)
        {
            var tags = item.VideoTags;
            return (
                tags?.FirstOrDefault(x => x.Level == 1)?.TagName,
                tags?.FirstOrDefault(x => x.Level == 2)?.TagName,
                tags?.FirstOrDefault(x => x.Level == 3)?.TagName
            );
        }
        /// <summary>
        /// AOP
        /// </summary>
        /// <returns></returns>
        protected virtual Task BeforeProcessCookies() => Task.CompletedTask;

        protected abstract Task<List<DouyinUserCookie>> GetValidCookies();
        protected abstract bool IsCookieValid(DouyinUserCookie cookie);
        protected abstract Task<DouyinVideoInfo> FetchVideoData(DouyinUserCookie cookie, string cursor);
        protected abstract bool ShouldContinueSync(DouyinUserCookie cookie, DouyinVideoInfo data);
        protected abstract string GetNextCursor(DouyinVideoInfo data);
        protected abstract string CreateSaveFolder(DouyinUserCookie cookie, Aweme item, string tag1, string tag2);
        protected abstract string GetVideoFileName(DouyinUserCookie cookie, Aweme item, VideoBitRate bitRate);
        protected abstract string GetAuthorAvatarBasePath(DouyinUserCookie cookie);
        protected abstract Task HandleSyncCompletion(DouyinUserCookie cookie, int syncCount);
        protected abstract VideoEntityDifferences GetVideoEntityDifferences(DouyinUserCookie cookie, Aweme item);

    }

    public class VideoEntityDifferences
    {
        public VideoTypeEnum VideoType { get; set; }
        public string VideoTitleSimplify { get; set; } = string.Empty;
    }
}