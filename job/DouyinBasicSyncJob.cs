using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.service;
using dy.net.utils;
using Newtonsoft.Json;
using Quartz;
using Serilog;
using System.Net;

namespace dy.net.job
{
    /// <summary>
    /// 抖音数据同步任务基类
    /// 所有具体的抖音同步任务（如收藏、关注、作品等）都应继承此类
    /// 提供了通用的同步逻辑，如Cookie处理、视频下载、数据存储等
    /// </summary>
    [DisallowConcurrentExecution] // 禁止并发执行，确保同一时间只有一个实例在运行
    public abstract class DouyinBasicSyncJob : IJob, IDisposable
    {
        #region 受保护字段

        /// <summary>
        /// 抖音Cookie服务，用于获取和管理用户Cookie
        /// </summary>
        protected readonly DouyinCookieService douyinCookieService;
        /// <summary>
        /// 抖音HTTP客户端服务，用于发送HTTP请求
        /// </summary>
        protected readonly DouyinHttpClientService douyinHttpClientService;
        /// <summary>
        /// 抖音视频服务，用于视频信息的数据库操作
        /// </summary>
        protected readonly DouyinVideoService douyinVideoService;
        /// <summary>
        /// 抖音通用服务，用于获取应用配置等
        /// </summary>
        protected readonly DouyinCommonService douyinCommonService;
        /// <summary>
        /// 抖音关注列表
        /// </summary>
        private readonly DouyinFollowService douyinFollowService;
        /// <summary>
        /// 图文合成视频
        /// </summary>
        private readonly DouyinMergeVideoService douyinMergeVideoService;
        /// <summary>
        /// 收藏夹、短剧、合集
        /// </summary>
        private readonly DouyinCollectCateService douyinCollectCateService;
        /// <summary>
        /// 随机数生成器，用于生成随机延迟，模拟人类操作
        /// </summary>
        protected readonly Random _random = new Random();
        /// <summary>
        /// 每页请求的视频数量.不可修改
        /// </summary>
        protected string count = "18";
        private bool disposedValue;

        #endregion

        #region 私有字段

        // 类级别静态字段：映射视频类型与同步完成状态判断逻辑（复用+易维护）
        //private static readonly Dictionary<VideoTypeEnum, Func<DouyinCookie, bool>> _syncStatusCheckMap = new()
        //{
        //    [VideoTypeEnum.dy_favorite] = cookie => cookie.FavHasSyncd == 1,
        //    [VideoTypeEnum.dy_collects] = cookie => cookie.CollHasSyncd == 1,
        //    [VideoTypeEnum.dy_follows] = cookie => cookie.UperSyncd == 1,
        //    [VideoTypeEnum.ImageVideo] = cookie => cookie.UperSyncd == 1
        //                                          && cookie.CollHasSyncd == 1
        //                                          && cookie.FavHasSyncd == 1
        //};

        #endregion

        #region 抽象属性

        /// <summary>
        /// 同步类型
        /// </summary>
        protected abstract VideoTypeEnum VideoType { get; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化 <see cref="DouyinBasicSyncJob"/> 类的新实例
        /// </summary>
        /// <param name="douyinCookieService">抖音Cookie服务</param>
        /// <param name="douyinHttpClientService">抖音HTTP客户端服务</param>
        /// <param name="douyinVideoService">抖音视频服务</param>
        /// <param name="douyinCommonService">抖音通用服务</param>
        /// <param name="douyinFollowService">抖音关注的</param>
        /// <param name="douyinMergeVideoService">视频合成</param>
        /// <param name="douyinCollectCateService"></param>
        protected DouyinBasicSyncJob(
            DouyinCookieService douyinCookieService,
            DouyinHttpClientService douyinHttpClientService,
            DouyinVideoService douyinVideoService,
            DouyinCommonService douyinCommonService,
            DouyinFollowService douyinFollowService,
            DouyinMergeVideoService douyinMergeVideoService,
            DouyinCollectCateService douyinCollectCateService)
        {
            this.douyinCookieService = douyinCookieService ?? throw new ArgumentNullException(nameof(douyinCookieService));
            this.douyinHttpClientService = douyinHttpClientService ?? throw new ArgumentNullException(nameof(douyinHttpClientService));
            this.douyinVideoService = douyinVideoService ?? throw new ArgumentNullException(nameof(douyinVideoService));
            this.douyinCommonService = douyinCommonService ?? throw new ArgumentNullException(nameof(douyinCommonService));
            this.douyinFollowService = douyinFollowService;
            this.douyinMergeVideoService = douyinMergeVideoService;
            this.douyinCollectCateService = douyinCollectCateService;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 执行任务的主入口点
        /// 由Quartz调度器调用，负责协调整个同步流程
        /// </summary>
        /// <param name="context">作业执行上下文</param>
        /// <returns>一个表示异步操作的任务</returns>
        public async Task Execute(IJobExecutionContext context)
        {
       
            // 获取应用配置
            var config = douyinCommonService.GetConfig();
            // 在处理Cookie之前执行的预处理
            await BeforeProcessCookies();

            // 获取所有有效的Cookie
            var cookies = await GetSyncCookies();
            if (cookies == null || !cookies.Any())
            {
                Log.Debug($"[{VideoType.GetDesc()}]-Cookie无效或已关闭同步...");
                return;
            }
            Log.Debug($"[{VideoType.GetDesc()}]共发现{cookies.Count}个有效Cookie，同步开始...");

            // 遍历每个有效的Cookie，执行同步
            foreach (var cookie in cookies)
            {
                await ProcessSyncUserCookie(cookie, config);
            }
        }

        #endregion

        #region 受保护方法

        /// <summary>
        /// 在处理Cookie之前执行的预处理操作-AOP
        /// </summary>
        /// <returns>一个表示异步操作的任务</returns>
        protected virtual Task BeforeProcessCookies() => Task.CompletedTask;

        /// <summary>
        /// 获取所有有效的Cookie
        /// 子类必须实现此方法，根据具体任务类型筛选有效的Cookie
        /// </summary>
        /// <returns>有效的Cookie列表</returns>
        protected virtual async Task<List<DouyinCookie>> GetSyncCookies()
        {
            return await douyinCookieService.GetOpendCookiesAsync(x => !string.IsNullOrWhiteSpace(x.SavePath));
        }

        /// <summary>
        /// 根据Cookie和游标获取视频数据
        /// 子类必须实现此方法，调用具体的API接口获取视频列表
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="cursor">分页游标，用于获取下一页数据</param>
        /// <param name="followed">关注的人</param>
        /// <param name="cate">自定义收藏夹、合集、短剧</param>
        /// <returns>视频信息对象，包含视频列表和分页信息</returns>
        protected abstract Task<DouyinVideoInfoResponse> FetchVideoData(DouyinCookie cookie, string cursor, DouyinFollowed followed, DouyinCollectCate cate);


        /// <summary>
        /// 获取下一页数据的游标
        /// </summary>
        /// <param name="data">当前获取到的视频数据</param>
        /// <returns>下一页数据的游标</returns>
        private static string GetNextCursor(DouyinVideoInfoResponse data)
        {
            return data?.Cursor ?? (data?.MaxCursor ?? "0");
        }


        /// <summary>
        /// 创建视频保存文件夹
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <param name="followed">关注用户</param>
        /// <param name="cate"></param>
        /// <param name="config">应用配置</param>
        /// <returns>创建的视频保存文件夹路径</returns>
        protected virtual string CreateSaveFolder(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed, DouyinCollectCate cate)
        {
            var subFolder = DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId, true);
            var folder = Path.Combine(cookie.SavePath, subFolder);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                //说明文件夹存在，检查里面有没有文件，如果已经有视频文件了，说明视频标题相同，那么应该重新创建文件夹,+id

                folder = Path.Combine(cookie.SavePath, subFolder + "_" + item.AwemeId);
            }
            return folder;

        }

        /// <summary>
        /// 获取视频文件名,默认就用id作文件名
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <param name="config">配置信息</param>
        /// <param name="cate"></param>
        /// <returns>生成的视频文件名</returns>
        protected virtual string GetVideoFileName(DouyinCookie cookie, Aweme item, AppConfig config, DouyinCollectCate cate)
        {
            if (cate != null && cate.CateType == VideoTypeEnum.dy_custom_collect)
            {
                if (item.Video != null && item.Video.BitRate != null)
                    return $"{item.AwemeId}.{item.Video.BitRate.FirstOrDefault().Format}";
                return $"{item.AwemeId}.mp4";
            }
            else
            {
                if ((VideoType == VideoTypeEnum.dy_series || VideoType == VideoTypeEnum.dy_mix) && item.MixInfo?.Statis?.CurrentEpisode != null)
                {
                    return $"{item.MixInfo.Statis.CurrentEpisode}.mp4";
                }
                return $"{item.AwemeId}.mp4";
            }
        }
        /// <summary>
        /// 获取作者头像保存的基础路径
        /// 子类必须实现此方法，指定头像的存储位置
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <returns>作者头像保存的基础路径</returns>
        protected abstract string GetAuthorAvatarBasePath(DouyinCookie cookie);

        /// <summary>
        /// 处理同步完成后的操作
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="syncCount">本次同步成功的视频数量</param>
        /// <param name="followed"></param>
        /// <param name="cate"></param>
        /// <returns>一个表示异步操作的任务</returns>
        protected async Task HandleSyncCompletion(DouyinCookie cookie, int syncCount, DouyinFollowed followed = null, DouyinCollectCate cate = null)
        {
            var tag = cate?.Name ?? followed?.UperName ?? string.Empty;
            tag = !string.IsNullOrWhiteSpace(tag) ? $"-[{tag}]" : tag;

            if (VideoType != VideoTypeEnum.dy_custom_collect || cookie.UseCollectFolder)
            {
                Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]{tag},本次成功同步{syncCount}条视频");
            }

            if (cate != null)
            {
                //更新合集短剧完结状态
                await douyinCollectCateService.UpdateCate2EndStatus(cate);
            }
        }

        /// <summary>
        /// 处理单个用户Cookie的同步逻辑
        /// 负责循环获取视频数据、处理视频、保存视频信息等
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="config">应用配置</param>
        /// <returns>一个表示异步操作的任务</returns>
        protected async Task ProcessSyncUserCookie(DouyinCookie cookie, AppConfig config)
        {
            try
            {
                switch (VideoType)
                {
                    case VideoTypeEnum.dy_follows:
                        if(cookie.DownFollowd)
                        {
                            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]开始同步...");
                            //查询关注列表开启了同步的关注
                            var follows = await douyinFollowService.GetSyncFollows(cookie.MyUserId);
                            if (follows != null && follows.Any())
                            {
                                foreach (var followed in follows)
                                {
                                    int syncCount = 0; // 本次同步成功的视频数量
                                    string cursor = "0";
                                    bool hasMore = true;
                                    (syncCount, cursor, hasMore) = await GetAndSaveViedos(cookie, config, syncCount, cursor, hasMore, followed);
                                    await HandleSyncCompletion(cookie, syncCount, followed);
                                }
                            }
                        }
                        else
                        {
                            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]同步未开启");
                        }
                        break;
                    case VideoTypeEnum.dy_favorite:
                        if(cookie.DownFavorite)
                        {
                            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]开始同步...");
                            int syncCount = 0;
                            string cursor = "0";
                            bool hasMore = true;
                            (syncCount, cursor, hasMore) = await GetAndSaveViedos(cookie, config, syncCount, cursor, hasMore);
                            await HandleSyncCompletion(cookie, syncCount);
                        }
                        else
                        {
                            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]同步未开启");
                        }
                        break;
                    case VideoTypeEnum.dy_collects:
                        if (cookie.UseCollectFolder)
                        {
                            Log.Debug($"[{VideoType.GetDesc()}]-已开启自定义收藏夹同步...break;");
                        }
                        else if (cookie.DownCollect)
                        {
                            int syncCount = 0;
                            string cursor = "0";
                            bool hasMore = true;
                            (syncCount, cursor, hasMore) = await GetAndSaveViedos(cookie, config, syncCount, cursor, hasMore);
                            await HandleSyncCompletion(cookie, syncCount);
                        }
                        else
                        {
                            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]同步未开启");
                        }
                        break;

                    case VideoTypeEnum.dy_mix:
                        if (cookie.DownMix)
                        {
                            await SyncCustomListVideos(cookie, config);
                        }
                        else
                        {
                            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]同步未开启");
                        }
                        break;
                    case VideoTypeEnum.dy_series:
                        if (cookie.DownSeries)
                            await SyncCustomListVideos(cookie, config);
                        else
                        {
                            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]同步未开启");
                        }
                        break;
                    case VideoTypeEnum.dy_custom_collect:
                        if (cookie.UseCollectFolder)
                            await SyncCustomListVideos(cookie, config);
                        else
                        {
                            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]同步未开启");
                        }
                        break;
                    case VideoTypeEnum.ImageVideo:
                    default:
                        break;
                }


            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{cookie.UserName}][{VideoType.GetDesc()}]同步出错!!!,{ex.StackTrace}");
            }
        }
        /// <summary>
        /// 同步下载自定义收藏夹、合集、短剧
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private async Task SyncCustomListVideos(DouyinCookie cookie, AppConfig config)
        {
            var cates = await douyinCollectCateService.GetSyncCates(cookie.Id, VideoType);
            if (cates != null && cates.Any())
            {
                foreach (var cate in cates)
                {
                    int syncCount = 0; // 本次同步成功的视频数量
                    string cursor = "0";
                    bool hasMore = true;
                    (syncCount, cursor, hasMore) = await GetAndSaveViedos(cookie, config, syncCount, cursor, hasMore, null, cate);
                    await HandleSyncCompletion(cookie, syncCount, null);

                }
            }
            else
            {
                Serilog.Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]没有查询到已开启的对象");
            }
        }

        private async Task<(int syncCount, string cursor, bool hasMore)> GetAndSaveViedos(DouyinCookie cookie, AppConfig config, int syncCount, string cursor, bool hasMore, DouyinFollowed followed = null, DouyinCollectCate cate = null)
        {
            // 循环获取视频数据
            while (hasMore)
            {
                // 获取视频数据
                var data = await FetchVideoData(cookie, cursor, followed, cate);
                if (data == null || data.AwemeList == null || !data.AwemeList.Any())
                {
                    Serilog.Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}] 没有新的视频");
                    break;
                }

                if (data.StatusCode == 0 && cookie.StatusMsg != "正常")
                {
                    cookie.StatusCode = data.StatusCode;
                    cookie.StatusMsg = "正常";
                    await douyinCookieService.UpdateAsync(cookie);
                }
                // 判断是否还有更多数据
                //hasMore = ShouldContinueSync(cookie, data, followed, config);

                // 获取下一页游标
                cursor = GetNextCursor(data);

                hasMore = data.HasMore == 1;

                // 处理视频列表
                (List<DouyinVideo> videos, int syncCountx) = await ProcessVideoList(syncCount, cookie, data, config, followed, cate);
                if (videos != null && videos.Any())
                {
                    // 保存视频信息到数据库
                    await SaveVideos(cookie,videos);
                }

                syncCount += syncCountx;

                if (IsSyncLimitReached(cookie, config, syncCount, cate, followed))
                {
                    break;
                }
                //随机等待
                await Task.Delay(_random.Next(2, 10) * 1000);
            }

            return (syncCount, cursor, hasMore);
        }


        /// <summary>
        /// 检查是否达到同步批次上限且满足状态条件，需要终止循环
        /// </summary>
        /// <param name="cookie">抖音Cookie</param>
        /// <param name="config">同步配置</param>
        /// <param name="syncCount">已同步数量</param>
        /// <param name="cate"></param>
        /// <param name="followed"></param>
        /// <returns>是否需要终止循环</returns>
        private bool IsSyncLimitReached(DouyinCookie cookie, AppConfig config, int syncCount, DouyinCollectCate cate, DouyinFollowed followed)
        {
            if (cate != null && cate.CateType != VideoTypeEnum.dy_custom_collect)
            {
                if (syncCount >= 15)
                {
                    Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]:本次同步数量{syncCount}，等下次任务继续同步");
                    return true;
                }

            }
            else
            {
                if (syncCount >= config.BatchCount)
                {
                    Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]:本次同步数量{syncCount}，已达配置上限{config.BatchCount}，等下次任务继续同步");
                    return true;
                }

            }
            if (VideoType == VideoTypeEnum.dy_collects || VideoType == VideoTypeEnum.dy_favorite)
                return config.OnlySyncNew;

            return VideoType == VideoTypeEnum.dy_follows && !followed.FullSync;

            //// 获取当前视频类型对应的状态判断逻辑
            //if (!_syncStatusCheckMap.TryGetValue(VideoType, out var isSyncCompleted))
            //{
            //    //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]无匹配的同步状态判断规则，不终止循环");
            //    return false;
            //}

            //// 状态满足则记录日志并返回「终止循环」
            //if (isSyncCompleted.Invoke(cookie))
            //{
            //    Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]本次同步达到上限{config.BatchCount}，停止同步!!!");
            //    return true;
            //}

        }
        /// <summary>
        /// 遍历视频列表，分别处理每个视频和图片集
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="data">视频信息对象</param>
        /// <param name="config">应用配置</param>
        /// <param name="followed">关注的</param>
        /// <param name="cate">收藏夹、合集、短剧</param>
        /// <returns>处理后的视频实体列表</returns>
        protected async Task<(List<DouyinVideo> videos, int currentCount)> ProcessVideoList(int syncCount1, DouyinCookie cookie, DouyinVideoInfoResponse data, AppConfig config, DouyinFollowed followed = null, DouyinCollectCate cate = null)
        {
            int syncCount = 0;
            var videos = new List<DouyinVideo>();
            foreach (var item in data.AwemeList)
            {
                //if (item.AwemeId != "7565895895016295726")
                //{
                //    continue;
                //}
                //if (!item.Desc.Contains("抖音各种隐藏功能，每个都好用到离谱#抖音#隐藏功能"))
                //{
                //    continue;
                //}

                //判断视频是否是强制删除且不再下载的视频
                var deleteVideo = await douyinCommonService.ExistDeleteVideo(item.AwemeId);
                if (deleteVideo)
                {
                    //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-视频-{item.AwemeId}-[{item.Desc}]已被标记为强制删除，跳过下载");
                    continue;
                }

                // 查询数据库中是否已存在该视频（通过 AwemeId 唯一标识）
                var exitVideo = await douyinVideoService.GetByAwemeId(item.AwemeId);

                bool Goon = await AutoDistinct(config, exitVideo, cookie);
                if (!Goon)
                {
                    continue;
                }

                if (exitVideo != null)
                {
                    //文件存在
                    if (File.Exists(exitVideo.VideoSavePath))
                    {
                        //如果当前时正在下载关注列表的视频，但是已经存在合集下载过了，那么跳过
                        if (VideoType == VideoTypeEnum.dy_follows && exitVideo.ViedoType == VideoTypeEnum.dy_mix)
                        {
                            continue;
                        }
                        else
                        {
                            //如果当前正在下载合集视频，但是发现已经存在了，但视频类型不是合集，那么删掉原来的记录以及文件，重新下载，合集优先级最高
                            if (VideoType == VideoTypeEnum.dy_mix && exitVideo.ViedoType != VideoTypeEnum.dy_mix)
                            {
                                File.Delete(exitVideo.VideoSavePath);
                                await douyinVideoService.DeleteById(exitVideo.Id);
                            }
                            else
                            {
                                await douyinVideoService.DeleteById(exitVideo.Id);
                            }
                        }
                    }
                    else
                    {
                        //文件不存在，直接删掉原始记录
                        await douyinVideoService.DeleteById(exitVideo.Id);
                    }
                }

                var uper = await douyinFollowService.GetByUperId(item.AuthorUserId.ToString(), cookie.MyUserId);
                if (uper != null && uper.FullSync)
                {
                    followed ??= uper;
                }
                // 处理单个视频
                var video = await ProcessSingleVideo(cookie, item, config, followed, cate);
                if (video != null)
                {
                    videos.Add(video);
                    syncCount++;

                    if (syncCount + syncCount1 >= config.BatchCount)
                    {
                        return (videos, syncCount);
                    }
                }
                else
                {
                    //处理多个视频-组合的图文视频--类似动图。
                    List<DouyinDynamicVideoDto> dynamicVideoUrls = new List<DouyinDynamicVideoDto>();
                    // 当需要下载动态视频时，获取其他URL
                    if (config.DownDynamicVideo && item.Images != null && item.Images.Count > 0)
                    {
                        foreach (var img in item.Images)
                        {
                            if (img.DynamicVideo?.BitRate?.Count > 0)
                            {
                                foreach (var btv in img.DynamicVideo.BitRate)
                                {
                                    var targetUrl = btv.PlayAddr?.UrlList?.FirstOrDefault(x => x.StartsWith("https://www.douyin.com/aweme/v1/play"));
                                    if (targetUrl != null)
                                    {
                                        var height = btv.PlayAddr?.Height ?? 1920;
                                        var width = btv.PlayAddr?.Width ?? 1080;
                                        DouyinDynamicVideoDto info = new DouyinDynamicVideoDto
                                        {
                                            Path = targetUrl,
                                            Height = height,
                                            Width = width
                                        };
                                        dynamicVideoUrls.Add(info);
                                    }
                                }
                            }
                        }
                    }

                    // 处理核心逻辑
                    if (dynamicVideoUrls.Count > 0)
                    {
                        // 处理动态视频
                        var dynamicVideo = await ProcessDynamicVideo(dynamicVideoUrls, cookie, item, config, followed, cate);
                        if (dynamicVideo != null)
                        {
                            if (!string.IsNullOrEmpty(dynamicVideo.DynamicVideos))
                            {
                                var dynamicVideos = JsonConvert.DeserializeObject<List<DouyinDynamicVideoDto>>(dynamicVideo.DynamicVideos);
                                Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]-动态视频[{item.Desc}]，下载成功 ,共{dynamicVideos?.Count}个视频...");
                                if (dynamicVideos != null && dynamicVideos.Count > 0)
                                {

                                    var savePath = DouyinFileNameHelper.RemoveNumberSuffix(dynamicVideo.VideoSavePath);

                                    //音频文件下载地址
                                    var mp3Url = item?.Music?.PlayUrl?.UrlList?.FirstOrDefault();


                                    var (mp4Path, mp3Path) = await douyinMergeVideoService.MergeMultipleVideosAsync(dynamicVideos, mp3Url, savePath, cookie.Cookies);
                                    if (!string.IsNullOrWhiteSpace(mp4Path))
                                    {
                                        if (File.Exists(mp4Path))
                                        {
                                            dynamicVideo.VideoSavePath = mp4Path;
                                            if (!config.KeepDynamicVideo)
                                            {
                                                if (File.Exists(mp3Path))
                                                {
                                                    File.Delete(mp3Path);
                                                }
                                                //不保留原视频-删除
                                                foreach (var opath in dynamicVideos)
                                                {
                                                    if (File.Exists(opath.Path))
                                                        File.Delete(opath.Path);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //动态视频生成nfo
                            NfoFileGenerator.GenerateVideoNfoFile(dynamicVideo);
                            videos.Add(dynamicVideo);
                            syncCount++;
                            if (syncCount + syncCount1 >= config.BatchCount)
                            {
                                return (videos, syncCount);
                            }
                        }
                        else
                        {
                            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]-动态视频[{item.Desc}]，下载失败...");
                        }
                    }
                    else
                    {
                        // 处理图文视频逻辑
                        if (config.DownImageVideo || config.DownMp3 || config.DownImage)
                        {
                            var mergevideo = await ProcessImageSetAndMergeToVideo(cookie, item, config, followed, cate);
                            if (mergevideo != null)
                            {
                                videos.Add(mergevideo);
                                syncCount++;
                                if (syncCount + syncCount1 >= config.BatchCount)
                                {
                                    return (videos, syncCount);
                                }
                            }
                        }
                    }
                }
            }
            return (videos, syncCount);
        }

        private async Task<bool> AutoDistinct(AppConfig config, DouyinVideo exitVideo,DouyinCookie cookie)
        {
            // 去重，检查视频是否已存在（按优先级下载）
            if (config.AutoDistinct)
            {
                if (exitVideo != null)
                {
                    // 2. 已存在视频：先判断本地文件是否存在
                    if (File.Exists(exitVideo.VideoSavePath))
                    {
                        List<PriorityLevelDto> priLevs = new List<PriorityLevelDto>();
                        if (!string.IsNullOrWhiteSpace(config.PriorityLevel))
                        {
                            priLevs = JsonConvert.DeserializeObject<List<PriorityLevelDto>>(config.PriorityLevel);
                        }

                        // 4. 处理优先级：获取「最高优先级」（Sort 越小优先级越高）
                        PriorityLevelDto maxPriority = null;
                        if (priLevs.Any())
                        {
                            // 前端已配置优先级：取 Sort 最小的（1最高）
                            maxPriority = priLevs.OrderBy(x => x.Sort).FirstOrDefault();
                        }
                        else
                        {
                            // 前端未配置：使用默认优先级（喜欢 > 收藏 > 关注）
                            maxPriority = new PriorityLevelDto { Id = 1, Sort = 1, Name = "喜欢的" }; // 默认「喜欢的视频」最高
                        }

                        // 5. 转换为当前上下文的视频类型
                        var maxPriorityType = (VideoTypeEnum)maxPriority.Id; // 配置的最高优先级类型

                        // 6. 获取已存在视频的类型（从数据库中 exitVideo 读取，需确保字段存在）
                        var exitVideoType = exitVideo.ViedoType; // 假设数据库存储了 VideoType.GetVideoTypeDesc()（1/2/3）

                        // 7. 优先级逻辑判断（核心）
                        if (VideoType == maxPriorityType)
                        {
                            // 情况1：当前要下载的是「最高优先级」视频
                            if (exitVideoType == VideoType)
                            {
                                // 已存在同优先级视频 → 跳过下载（避免重复）
                                //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-视频-{exitVideo.AwemeId}-[{exitVideo.VideoTitle}]已存在（同最高优先级），跳过");
                                return false;
                            }
                            else
                            {
                                // 已存在「低优先级」视频 → 替换（删除旧文件，继续下载新的最高优先级视频）
                                //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-视频-{exitVideo.AwemeId}-[{exitVideo.VideoTitle}]已存在（低优先级：{exitVideoType.GetVideoTypeDesc()}），替换为最高优先级：{currentVideoType.GetVideoTypeDesc()}");

                                // 删除旧的低优先级文件（可选：也可保留备份，根据需求调整）
                                try
                                {
                                    //File.Delete(exitVideo.VideoSavePath);
                                    DeleteOldViedo(exitVideo);
                                    //Log.Debug($"已删除旧文件：{exitVideo.VideoSavePath}");
                                }
                                catch (Exception ex)
                                {
                                    Log.Error($"[{cookie.UserName}][{VideoType.GetDesc()}]-删除重复的文件[{exitVideo.VideoTitle}]失败：{ex.Message}", ex);
                                    // 即使删除失败，仍继续下载（新文件会覆盖旧文件，或按路径规则重命名）
                                }

                                // 继续执行下载逻辑（覆盖旧数据）
                            }
                        }
                        else
                        {
                            // 情况2：当前要下载的是「非最高优先级」视频
                            if (exitVideoType == maxPriorityType)
                            {
                                // 已存在「最高优先级」视频 → 跳过（不替换最高优先级）
                                //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-视频-{exitVideo.AwemeId}-[{exitVideo.VideoTitle}]已存在最高优先级视频（{maxPriorityType}），当前类型（{currentVideoType.GetVideoTypeDesc()}）优先级低，跳过");
                                return false;
                            }
                            else
                            {
                                // 已存在「其他非最高优先级」视频 → 比较两者优先级
                                // 获取当前类型和已存在类型的 Sort 值
                                var currentSort = priLevs.FirstOrDefault(x => x.Id == (int)VideoType)?.Sort ?? int.MaxValue;
                                var exitSort = priLevs.FirstOrDefault(x => x.Id == (int)exitVideoType)?.Sort ?? int.MaxValue;

                                if (currentSort < exitSort)
                                {
                                    // 当前类型优先级更高 → 替换旧视频
                                    //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-视频-{exitVideo.AwemeId}-[{exitVideo.VideoTitle}]已存在低优先级视频（{exitVideoType.GetVideoTypeDesc()}），替换为当前优先级：{currentVideoType.GetVideoTypeDesc()}");
                                    // 删除旧文件
                                    DeleteOldViedo(exitVideo);
                                    // 继续下载
                                }
                                else
                                {
                                    // 当前类型优先级更低或相等 → 跳过
                                    //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-视频-{exitVideo.AwemeId}-[{exitVideo.VideoTitle}]已存在更高/同等优先级视频（{exitVideoType.GetVideoTypeDesc()}），当前类型（{currentVideoType.GetVideoTypeDesc()}）跳过");
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (exitVideo.OnlyImgOrOnlyMp3)
                        {
                            return true;//说明是图文视频，不需要再下载视频了
                        }
                        else
                        {
                            //记录存在，但本地文件不存在，则继续下载。
                            //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-视频-{exitVideo.AwemeId}记录存在，但本地文件缺失，删除记录，重新下载");
                            //删除原来的记录
                            await douyinVideoService.DeleteById(exitVideo.Id);
                        }
                    }
                }
            }
            return true;
        }

        private static void DeleteOldViedo(DouyinVideo exitVideo)
        {
            if (File.Exists(exitVideo.VideoSavePath))
            {
                var dirPath = Path.GetDirectoryName(exitVideo.VideoSavePath);
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                    //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-已删除旧文件夹：{dirPath}");
                }
                //查看是否还有其他文件，如果没有则删除文件夹
                var parentDir = Path.GetDirectoryName(exitVideo.VideoSavePath);
                if (Directory.Exists(parentDir) && !Directory.EnumerateFileSystemEntries(parentDir).Any())
                {
                    Directory.Delete(parentDir);
                    //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-已删除空文件夹：{parentDir}");
                }
            }
        }

        /// <summary>
        /// 处理单个视频--正常的收藏喜欢关注的 视频
        /// 负责下载视频、封面、头像，生成NFO文件，创建视频实体等
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <param name="config">应用配置</param>
        /// <param name="followed">关注</param>
        /// <param name="cate"></param>
        /// <returns>处理后的视频实体，如果处理失败则为null</returns>
        protected async Task<DouyinVideo> ProcessSingleVideo(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed = null, DouyinCollectCate cate = null)
        {
            // 检查视频数据是否有效
            if (!IsAwemeValid(item)) return null;
            // 获取视频最佳下载地址
            var v = GetBestMatchedVideoUrl(item, config);

            if (v == null)
            {
                Serilog.Log.Error($"[{cookie.UserName}][{VideoType.GetDesc()}][{item.Desc}]未获取到下载地址");
                return null;
            }
            //Serilog.Log.Debug($"{v.QualityType}-{v.BitRateValue}-{v.IsH265}-{v.HdrBit}");
            var videoUrl = v.PlayAddr.UrlList.Where(x => !string.IsNullOrEmpty(x))?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                Serilog.Log.Error($"[{cookie.UserName}][{VideoType.GetDesc()}]未获取到下载地址");
                return null;
            }
            // 创建保存文件夹
            var saveFolder = CreateSaveFolder(cookie, item, config, followed, cate);
            // 获取视频文件名
            var fileName = GetVideoFileName(cookie, item, config, cate);
            // 拼接视频保存路径
            var savePath = Path.Combine(saveFolder, fileName);
            var savePath2 = Path.Combine(saveFolder, DateTime.Now.Ticks + ".mp4");

            // 如果文件已存在，跳过
            if (File.Exists(savePath))
            {
                //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-视频[{DouyinFileNameHelper.SanitizePath(item.Desc)}]已存在，跳过下载.");
                return null;
            }

            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}][{item?.Author?.Nickname ?? ""}]-视频[{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId)}]开始下载...");
            // 随机延迟
            await Task.Delay(_random.Next(1, 4) * 1000);

            // 下载视频
            var (Success, _) = await douyinHttpClientService.DownloadAsync(videoUrl, savePath, cookie.Cookies);
            if (!Success)
            {
                Log.Error($"[{cookie.UserName}][{VideoType.GetDesc()}][{item?.Author?.Nickname ?? ""}]-视频[{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId)}]下载失败!!!");

                (bool flowControl, DouyinVideo value) = await SwitchOtherUrlAddressDown(cookie, item, videoUrl, savePath);
                if (!flowControl)
                {
                    return value;
                }
            }
            else
            {
                Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}][{item?.Author?.Nickname ?? ""}]-视频[{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId)}]下载完成.");
            }
            // 下载视频封面
            var coverSavePath=  await DownVideoCover(item, savePath, cookie, cate);
            // 下载作者头像
            var avatarSavePath = await DownAuthorAvatar(cookie, item);

            // 创建视频实体
            return await CreateVideoEntity(config, cookie, item, v, savePath, coverSavePath, avatarSavePath, null, cate);
        }

        private static VideoBitRate GetBestMatchedVideoUrl(Aweme item, AppConfig config)
        {
            VideoBitRate v;
            if (config.VideoEncoder.HasValue && config.VideoEncoder.Value == 265)
            {
                v = item.Video.BitRate.Where(v => v.IsH265 == 1 && v.PlayAddr?.UrlList != null && v.PlayAddr.UrlList.Any())
                                .OrderByDescending(v => v.BitRateValue)
                                .FirstOrDefault();
                v ??= item.Video.BitRate.Where(v => v.IsH265 == 0 && v.PlayAddr?.UrlList != null && v.PlayAddr.UrlList.Any())
                                .OrderByDescending(v => v.BitRateValue)
                                .FirstOrDefault();
            }

            else
            {
                v = item.Video.BitRate.Where(v => v.IsH265 == 0 && v.PlayAddr?.UrlList != null && v.PlayAddr.UrlList.Any())
                                  .OrderByDescending(v => v.BitRateValue)
                                  .FirstOrDefault();
            }

            return v;
        }

        /// <summary>
        /// 动态视频处理
        /// </summary>
        /// <param name="dynamicUrls"></param>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="config"></param>
        /// <param name="followed"></param>
        /// <param name="cate"></param>
        /// <returns></returns>
        protected async Task<DouyinVideo> ProcessDynamicVideo(List<DouyinDynamicVideoDto> dynamicUrls, DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed = null, DouyinCollectCate cate = null)
        {
            // 创建保存文件夹
            var saveFolder = CreateSaveFolder(cookie, item, config, followed, cate);
            // 获取视频文件名
            //var fileName = DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId) + ".mp4";

            var fileName = $"{item.AwemeId}.mp4";
            // 拼接视频保存路径
            var savePath = Path.Combine(saveFolder, fileName);

            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]-动态视频[{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId)}]开始下载...");
            // 随机延迟
            int i = 1;

            List<DouyinDynamicVideoDto> dynamicSavePaths = new List<DouyinDynamicVideoDto>();
            foreach (var dynamicUrl in dynamicUrls)
            {
                // 下载动态视频
                var dynamicSavePath = savePath.Replace(".mp4", $"_00{i}.mp4");
                DouyinDynamicVideoDto v = new DouyinDynamicVideoDto() { Height = dynamicUrl.Height, Width = dynamicUrl.Width, Path = dynamicSavePath };

                i++;
                // 如果文件已存在，跳过
                if (File.Exists(dynamicSavePath))
                {
                    //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-视频[{DouyinFileNameHelper.SanitizePath(item.Desc)}]已存在，跳过下载.");
                    dynamicSavePaths.Add(v);
                    continue;
                }
                var (Success, ActualSavePath) = await douyinHttpClientService.DownloadAsync(dynamicUrl.Path, dynamicSavePath, cookie.Cookies);
                if (!Success)
                {
                    Log.Error($"[{cookie.UserName}][{VideoType.GetDesc()}][{item?.Author?.Nickname ?? ""}]-动态视频[{dynamicSavePath}]-00{i},下载失败!!!");
                }
                else
                {
                    dynamicSavePaths.Add(v);
                    Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}][{item?.Author?.Nickname ?? ""}]-动态视频[{dynamicSavePath}]-00{i},下载完成.");
                }
                await Task.Delay(_random.Next(2, 10) * 1000);
            }


            // 下载视频封面
           var coverSavePath = await DownVideoCover(item, savePath, cookie, cate);
            // 下载作者头像
            var avatarSavePath = await DownAuthorAvatar(cookie, item);

            // 创建视频实体
            var virtualBitRate = new VideoBitRate
            {
                PlayAddr = new PlayAddr
                {
                    Width = item?.Images?.FirstOrDefault()?.Width ?? 0,
                    Height = item?.Images?.FirstOrDefault()?.Height ?? 0,
                    DataSize = DouyinFileUtils.GetTotalFileSize(dynamicSavePaths.Select(x => x.Path).ToList())  // 合成视频的文件大小
                }
            };
            return await CreateVideoEntity(config, cookie, item, virtualBitRate, dynamicSavePaths.FirstOrDefault()?.Path, coverSavePath, avatarSavePath, dynamicSavePaths, cate);
        }


        /// <summary>
        /// 寻找其他视频地址下载...
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="item"></param>
        /// <param name="videoUrl"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        private async Task<(bool flowControl, DouyinVideo value)> SwitchOtherUrlAddressDown(DouyinCookie cookie, Aweme item, string videoUrl, string savePath)
        {
            Log.Debug($"[ {VideoType.GetDesc()} ][ {item?.Author?.Nickname ?? ""} ]-视频[{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId)}],正在尝试切换其他地址再次重新下载...");
            var otherUrls = new List<string>();

            foreach (var bit in item.Video.BitRate)
            {
                if (bit == null)
                {
                    continue;
                }
                var payUrls = bit.PlayAddr;

                if (payUrls == null || payUrls.UrlList == null || payUrls.UrlList.Count == 0)
                {
                    continue;
                }

                foreach (var payurl in payUrls.UrlList)
                {
                    if (payurl == videoUrl)//   排除最开始下载失败的视频地址
                        continue;
                    otherUrls.Add(payurl);
                }
            }

            if (otherUrls.Count > 0)
            {
                //Log.Debug($"[{VideoType.GetVideoTypeDesc()}]-额外找到{otherUrls.Count}个视频地址，即将再次开始下载...");
                var (Success, ActualSavePath) = await douyinHttpClientService.DownloadAsync(videoUrl, savePath, cookie.Cookies, otherUrls);
                if (Success)
                {
                    Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]-尝试多个地址后，下载成功,{savePath}");
                }
                else
                {
                    Log.Error($"[{cookie.UserName}][{VideoType.GetDesc()}]-尝试多个地址后，依旧下载失败,{item.Desc}");
                    return (flowControl: false, value: null);
                }
            }
            else
            {
                Log.Error($"[{cookie.UserName}][{VideoType.GetDesc()}]-没有找到额外可以尝试下载链接了," + item.Desc);
                return (flowControl: false, value: null);
            }

            return (flowControl: true, value: null);
        }

        /// <summary>
        /// 负责下载图片、合成视频、处理封面和头像等
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息（包含图片集）</param>
        /// <param name="config">应用配置</param>
        /// <param name="followed">应用配置</param>
        /// <param name="cate"></param>
        /// <returns>合成后的视频实体，如果处理失败则为null</returns>
        protected async Task<DouyinVideo> ProcessImageSetAndMergeToVideo(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed, DouyinCollectCate cate)
        {
            try
            {
                // 提取图片URL列表
                List<string> imageUrls = item.Images?
                .Where(img => img.UrlList != null && img.UrlList.Any())
                .Select(img => img.UrlList.FirstOrDefault())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToList();

                // 如果没有图片，返回null
                if (imageUrls == null || !imageUrls.Any())
                {
                    return null;
                }

                // 创建图片保存文件夹
                var fileNamefolder = string.Empty;

                fileNamefolder = CreateSaveFolder(cookie, item, config, followed, cate);

                if (!Directory.Exists(fileNamefolder)) Directory.CreateDirectory(fileNamefolder);

                var fileName = GetVideoFileName(cookie, item, config, cate);
                // 合成视频的保存路径
                var savePath = Path.Combine(fileNamefolder, fileName);

                // 如果文件已存在，返回null
                if (File.Exists(savePath))
                {
                    FileInfo fileInfo = new FileInfo(savePath);
                    if (fileInfo.Length > 0)
                        return null;
                }

                // 获取音乐URL
                var mp3Url = item.Music?.PlayUrl?.UrlList?.FirstOrDefault();
                var firstImage = item.Images.FirstOrDefault();
                int height = firstImage.Height;
                int width = firstImage.Width;

                // 准备合成视频的请求参数
                var reqParams = new MediaMergeRequest
                {
                    ImageDurationPerSecond = 3, // 每张图片显示的时长（秒）
                    OutputFormat = "mp4", // 输出视频格式
                    VideoFps = 30, // 视频帧率
                    AudioUrls = string.IsNullOrWhiteSpace(mp3Url) ? new List<string>() : new List<string> { mp3Url }, // 音频URL列表
                    ImageUrls = imageUrls, // 图片URL列表
                    VideoWidth = width > 0 ? width : 1080, // 视频宽度
                    VideoHeight = height > 0 ? height : 1920, // 视频高度
                };

                // 执行图片合成视频操作
                var mergeResult = await douyinMergeVideoService.MergeToVideo(cookie.Cookies, AppContext.BaseDirectory, reqParams, savePath, fileNamefolder, config.DownImageVideo, config.DownImage, config.DownMp3);
                if (!mergeResult)
                {
                    Log.Error($"[{cookie.UserName}][{VideoType.GetDesc()}]-图文视频-[{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId)}]合成失败!!!");
                    return null;
                }


                // 获取不带扩展名的完整路径
                //string fullPathWithoutExtension = Path.Combine(Path.GetDirectoryName(savePath),Path.GetFileNameWithoutExtension(savePath) );
                if (config.DownImageVideo)
                {
                    // 检查合成后的视频文件是否有效
                    if (!File.Exists(savePath) || new FileInfo(savePath).Length <= 0)
                    {
                        Log.Error($"[{cookie.UserName}][{VideoType.GetDesc()}]-图文视频-[{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId)}]合成失败!!!");
                        // 清理无效的文件和文件夹
                        if (Directory.Exists(fileNamefolder))
                        {
                            File.Delete(savePath);
                            Directory.Delete(fileNamefolder, true);
                            Log.Error($"[{cookie.UserName}][{VideoType.GetDesc()}]-图文视频-删除合成失败的视频文件和目录...");
                        }
                        return null;
                    }
                }
                else
                {
                    savePath = "";
                }

                var coverUrl = cate is not null && cate.CateType != VideoTypeEnum.dy_custom_collect
              ? (item.MixInfo?.CoverUrl?.UrlList?.FirstOrDefault() ?? imageUrls.FirstOrDefault() ?? item.Music?.CoverHd?.UrlList?.FirstOrDefault())
              : imageUrls.FirstOrDefault();
               
                // 下载作者头像
                var avatarSavePath = await DownAuthorAvatar(cookie, item);

                // 为合成的视频创建一个“虚拟”的BitRate对象，以便复用CreateVideoEntity方法
                var virtualBitRate = new VideoBitRate
                {
                    PlayAddr = new PlayAddr
                    {
                        Width = reqParams.VideoWidth,
                        Height = reqParams.VideoHeight,
                        DataSize = string.IsNullOrWhiteSpace(savePath) || !File.Exists(savePath) ? 0 : new FileInfo(savePath).Length // 合成视频的文件大小
                    }
                };
                // 下载视频封面（使用第一张图片作为封面）
                string coverSavePath = await DownVideoCover(coverUrl, savePath, cookie);
             
                // 创建视频实体
                var videoEntity = await CreateVideoEntity(config,
                    cookie, item, virtualBitRate, coverSavePath, fileNamefolder, avatarSavePath, null, cate);

                // 特殊处理合成视频的字段
                videoEntity.FileHash = string.Empty; // 合成视频没有原始文件哈希
                videoEntity.VideoUrl = "/"; // 合成视频没有原始URL
                videoEntity.ViedoType = VideoType;
                videoEntity.IsMergeVideo = 1;// 标记为图片合成视频

        
                return videoEntity;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{cookie.UserName}][{VideoType.GetDesc()}]-图片视频同步-处理图片集并合成视频时出错");
                return null;
            }
        }

        /// <summary>
        /// 保存视频信息到数据库
        /// 批量插入视频实体列表到数据库中
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="videos">要保存的视频实体列表</param>
        /// <returns>保存成功的视频数量</returns>
        protected async Task<int> SaveVideos(DouyinCookie cookie, List<DouyinVideo> videos)
        {
            if (!videos.Any()) return 0;
            try
            {
                await douyinVideoService.BatchInsertOrUpdate(videos);
                return videos.Count;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{cookie.UserName}][{VideoType.GetDesc()}]-批量保存视频到数据库失败");
                // 清理保存失败的视频文件
                await CleanupFailedVideos(cookie,videos);
                return 0;
            }
        }

        /// <summary>
        /// 下载视频封面
        /// 从视频信息中提取封面URL并下载到指定文件夹
        /// </summary>
        /// <param name="item">视频信息</param>
        /// <param name="savePath">视频保存路径</param>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="cate"></param>
        /// <returns>一个表示异步操作的任务</returns>
        protected async Task<string> DownVideoCover(Aweme item, string savePath, DouyinCookie cookie,  DouyinCollectCate cate)
        {
            // 定义封面URL变量
            string coverUrl;

            // 按照优先级获取封面URL
            if (cate is not null)
            {
                // cate不为空时：优先MixInfo封面 → 其次Music高清封面 → 最后Video封面
                coverUrl = item.MixInfo?.CoverUrl?.UrlList?.FirstOrDefault()
                           ?? item.Video.Cover.UrlList?.LastOrDefault()
                           ?? item.Music?.CoverHd?.UrlList?.FirstOrDefault();
            }
            else
            {
                // cate为空时：只取Video封面
                coverUrl = item.Video?.Cover?.UrlList?.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(coverUrl))
                {
                    coverUrl = item.Images?.FirstOrDefault()?.DynamicVideo?.Cover?.UrlList?.FirstOrDefault();
                }
            }
            // 调用下载封面的方法
           return await DownVideoCover(coverUrl, savePath, cookie);
        }

        /// <summary>
        /// 下载作者头像
        /// 从视频信息中提取作者头像URL并下载到指定文件夹
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <returns>一个元组，包含头像保存路径和头像URL</returns>
        protected async Task<string> DownAuthorAvatar(DouyinCookie cookie, Aweme item)
        {
            if (item.Author == null) return string.Empty;
            // 优先获取高清头像
            var avatarUrl = item.Author.AvatarLarger?.UrlList?.FirstOrDefault() ?? item.Author.AvatarThumb?.UrlList?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(avatarUrl)) return  string.Empty;

            // 拼接头像保存路径
            var avatarSavePath = Path.Combine(GetAuthorAvatarBasePath(cookie), $"{item.Author.Uid}.jpg");
            var avatarDir = Path.GetDirectoryName(avatarSavePath);
            // 创建头像保存文件夹
            if (!Directory.Exists(avatarDir)) Directory.CreateDirectory(avatarDir);
            // 如果头像文件不存在，则下载
            if (!File.Exists(avatarSavePath))
            {
                await douyinHttpClientService.DownloadAsync(avatarUrl, avatarSavePath, cookie.Cookies);
            }
            return avatarSavePath;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 检查视频数据是否有效
        /// 验证视频信息是否包含必要的字段
        /// </summary>
        /// <param name="item">视频信息</param>
        /// <returns>如果视频数据有效，则为true；否则为false</returns>
        private static bool IsAwemeValid(Aweme item) => item != null && item.Video != null && item.Video.BitRate != null;

        /// <summary>
        /// 获取视频标签
        /// 从视频信息中提取三个级别的标签
        /// </summary>
        /// <param name="item">视频信息</param>
        /// <returns>一个元组，包含三个级别的视频标签</returns>
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
        /// 清理保存失败的视频文件
        /// 当数据库保存失败时，删除已下载的视频文件和文件夹
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="videos">保存失败的视频实体列表</param>
        /// <returns>一个表示异步操作的任务</returns>
        private async Task CleanupFailedVideos(DouyinCookie cookie, List<DouyinVideo> videos)
        {
            Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]-数据库保存失败，开始清理本次下载的视频目录...");

            foreach (var video in videos)
            {
                // 异步删除文件和文件夹，避免阻塞主线程
                await Task.Run(() =>
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(video.VideoSavePath) && File.Exists(video.VideoSavePath))
                        {
                            File.Delete(video.VideoSavePath);
                        }
                        string directory = Path.GetDirectoryName(video.VideoSavePath);
                        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory) && Directory.GetFileSystemEntries(directory).Length == 0)
                        {
                            Directory.Delete(directory);
                        }
                        Log.Debug($"[{cookie.UserName}][{VideoType.GetDesc()}]-清理失败视频文件成功: {video.VideoSavePath}!!!");

                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, $"[{cookie.UserName}][{VideoType.GetDesc()}]-清理失败视频文件出错: {video.VideoSavePath}!!!");
                    }
                });
            }
        }

        /// <summary>
        /// 下载视频封面（重载）
        /// 根据指定的封面URL下载封面图片，并复制为fanart.jpg
        /// </summary>
        /// <param name="coverUrl">封面图片URL</param>
        /// <param name="savePath">封面保存文件夹</param>
        /// <param name="cookie">用户Cookie</param>
        /// <returns>一个表示异步操作的任务</returns>
        private async Task<string> DownVideoCover(string coverUrl, string savePath, DouyinCookie cookie)
        {
            if (string.IsNullOrWhiteSpace(coverUrl)) return string.Empty;
            if (string.IsNullOrWhiteSpace(savePath)) return string.Empty;

            string directoryPath = Path.GetDirectoryName(savePath); // 获取文件所在目录，
            string newFileName = "poster.jpg";
            if (VideoType != VideoTypeEnum.dy_mix && VideoType != VideoTypeEnum.dy_series)
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(savePath); // 获取无后缀的原文件名，
                newFileName = $"{fileNameWithoutExt}-poster.jpg"; // 拼接新文件名，
            }

           var coverSavePath = Path.Combine(directoryPath, newFileName);


            // 如果封面文件不存在，则下载
            if (!File.Exists(coverSavePath))
            {
                await douyinHttpClientService.DownloadAsync(coverUrl, coverSavePath, cookie.Cookies);
            }
            return coverSavePath;
        }

        /// <summary>
        /// 创建视频实体
        /// 根据视频信息、下载路径等创建DouyinVideo实体对象
        /// </summary>
        /// <param name="config">配置</param>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <param name="bitRate">视频码率信息</param>
        /// <param name="savePath">视频保存路径</param>
        /// <param name="coverSavePath">海报保存路径</param>
        /// <param name="avatorPath"></param>
        /// <param name="dynamicVideos">动态视频</param>
        /// <param name="cate">短剧、合集、自定义收藏夹</param>
        /// <returns>创建的视频实体对象</returns>
        private async Task<DouyinVideo> CreateVideoEntity(AppConfig config,
            DouyinCookie cookie, Aweme item, VideoBitRate bitRate, string savePath, string coverSavePath, string avatorPath, List<DouyinDynamicVideoDto> dynamicVideos = null, DouyinCollectCate cate = null)
        {
            // 获取视频标签
            var (tag1, tag2, tag3) = GetVideoTags(item);
            var video = new DouyinVideo
            {
                ViedoType = VideoType,
                AwemeId = item.AwemeId,
                Author = item.Author?.Nickname,
                AuthorId = item.Author?.Uid,
                AuthorAvatar = avatorPath,
                AuthorAvatarUrl = item.Author.AvatarLarger?.UrlList?.FirstOrDefault() ?? item.Author.AvatarThumb?.UrlList?.FirstOrDefault(),
                CreateTime = DateTimeUtil.Convert10BitTimestamp(item.CreateTime),
                VideoTitle = string.IsNullOrWhiteSpace(item.Desc) ? $"{item.Author?.Nickname}-{item.CreateTime}" : item.Desc,
                //VideoTitleSimplify = VideoType == VideoTypeEnum.dy_follows? GetVideoSimplifyTitle(item):string.Empty,
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
                VideoCoverSavePath = coverSavePath,
                SyncTime = DateTime.Now,
                DyUserId = item.AuthorUserId == 0 ? item.Author?.Uid : item.AuthorUserId.ToString(),
                CookieId = cookie.Id,
                OnlyImgOrOnlyMp3 = string.IsNullOrWhiteSpace(savePath) && !config.DownImageVideo && (config.DownImage || config.DownMp3),
                CateId = cate?.Id,
                CateXId = cate?.XId,
            };
            if (cate != null && cate.CateType != VideoTypeEnum.dy_custom_collect)
            {
                video.VideoTitle = (string.IsNullOrWhiteSpace(item.Desc) ? cate.Name : $"[{cate.Name}]" + "_" + item.Desc) + "_" + item.MixInfo.Statis.CurrentEpisode;
            }
            if (dynamicVideos != null && dynamicVideos.Count > 0)
            {
                video.DynamicVideos = JsonConvert.SerializeObject(dynamicVideos);
            }
            else
            {
                // 生成NFO文件,动态视频 合成后重新生成，不在这里生成nfo
                NfoFileGenerator.GenerateVideoNfoFile(video);
            }

            return video;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~DouyinBasicSyncJob()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion


    }
}