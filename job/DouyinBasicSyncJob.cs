using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
using Newtonsoft.Json;
using Quartz;
using Quartz.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace dy.net.job
{
    /// <summary>
    /// 抖音数据同步任务基类
    /// 所有具体的抖音同步任务（如收藏、关注、作品等）都应继承此类
    /// 提供了通用的同步逻辑，如Cookie处理、视频下载、数据存储等
    /// </summary>
    [DisallowConcurrentExecution] // 禁止并发执行，确保同一时间只有一个实例在运行
    public abstract class DouyinBasicSyncJob : IJob
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
        /// 随机数生成器，用于生成随机延迟，模拟人类操作
        /// </summary>
        protected readonly Random _random = new Random();



        /// <summary>
        /// 每页请求的视频数量，可通过配置文件修改
        /// </summary>
        protected string count = "18";

        #endregion

        #region 私有字段

        /// <summary>
        /// 是否下载图片并合成视频
        /// </summary>
        //private bool _downImageVideo;

        #endregion

        #region 抽象属性

        /// <summary>
        /// 任务类型名称，用于日志记录和区分不同的同步任务
        /// 子类必须实现此属性并返回具体的任务类型
        /// </summary>
        protected abstract string JobType { get; }

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
        protected DouyinBasicSyncJob(
            DouyinCookieService douyinCookieService,
            DouyinHttpClientService douyinHttpClientService,
            DouyinVideoService douyinVideoService,
            DouyinCommonService douyinCommonService,
            DouyinFollowService douyinFollowService,
            DouyinMergeVideoService douyinMergeVideoService)
        {
            this.douyinCookieService = douyinCookieService ?? throw new ArgumentNullException(nameof(douyinCookieService));
            this.douyinHttpClientService = douyinHttpClientService ?? throw new ArgumentNullException(nameof(douyinHttpClientService));
            this.douyinVideoService = douyinVideoService ?? throw new ArgumentNullException(nameof(douyinVideoService));
            this.douyinCommonService = douyinCommonService ?? throw new ArgumentNullException(nameof(douyinCommonService));
            this.douyinFollowService = douyinFollowService;
            this.douyinMergeVideoService = douyinMergeVideoService;
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
            // 1. 获取应用配置
            var config = douyinCommonService.GetConfig();
            if (config == null)
            {
                Log.Debug($"{JobType}-未获取到系统配置，任务终止!!!");
                return;
            }


            // 2. 从配置中获取每页请求数量
            if (config.BatchCount > 0)
                count = config.BatchCount.ToString();

            // 3. 在处理Cookie之前执行的预处理操作
            await BeforeProcessCookies();

            // 4. 获取所有有效的Cookie
            var cookies = await GetValidCookies();
            if (cookies == null || !cookies.Any())
            {
                Log.Debug($"{JobType}-无有效Cookie，任务终止!!!");
                return;
            }

            Log.Debug($"{JobType}-共发现{cookies.Count}个Cookie，同步任务即将开始...");

            // 6. 遍历每个有效的Cookie，执行同步操作
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
        protected abstract Task<List<DouyinCookie>> GetValidCookies();

        /// <summary>
        /// 获取关注列表
        /// </summary>
        /// <returns></returns>
        protected virtual Task<List<DouyinFollowed>> GetFollows()
        {
            return Task.FromResult(new List<DouyinFollowed>());
        }

        /// <summary>
        /// 检查指定的Cookie是否有效
        /// </summary>
        /// <param name="cookie">要检查的Cookie</param>
        /// <returns>如果Cookie有效，则为true；否则为false</returns>
        protected virtual bool IsCookieValid(DouyinCookie cookie)
        {
            return !string.IsNullOrWhiteSpace(cookie.Cookies);
        }

        /// <summary>
        /// 根据Cookie和游标获取视频数据
        /// 子类必须实现此方法，调用具体的API接口获取视频列表
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="cursor">分页游标，用于获取下一页数据</param>
        /// <param name="uperUid">关注的人</param>
        /// <returns>视频信息对象，包含视频列表和分页信息</returns>
        protected abstract Task<DouyinVideoInfo> FetchVideoData(DouyinCookie cookie, string cursor, string uperUid = "");

        /// <summary>
        /// 判断是否应该继续同步下一页数据
        /// 子类必须实现此方法，根据API返回的分页信息判断是否还有更多数据
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="data">当前获取到的视频数据</param>
        /// <param name="followed">关注博主</param>
        /// <returns>如果应该继续同步，则为true；否则为false</returns>
        protected abstract bool ShouldContinueSync(DouyinCookie cookie, DouyinVideoInfo data, DouyinFollowed followed);

        /// <summary>
        /// 获取下一页数据的游标
        /// 子类必须实现此方法，从当前视频数据中提取下一页的游标
        /// </summary>
        /// <param name="data">当前获取到的视频数据</param>
        /// <returns>下一页数据的游标</returns>
        protected abstract string GetNextCursor(DouyinVideoInfo data);

        /// <summary>
        /// 创建视频保存文件夹
        /// 子类必须实现此方法，根据具体的存储策略创建文件夹
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <param name="followed">关注用户</param>
        /// <param name="config">应用配置</param>
        /// <returns>创建的视频保存文件夹路径</returns>
        protected abstract string CreateSaveFolder(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed);

        /// <summary>
        /// 获取视频文件名,默认就用id作文件名
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <param name="config">配置信息</param>
        /// <returns>生成的视频文件名</returns>
        protected virtual string GetVideoFileName(DouyinCookie cookie, Aweme item, AppConfig config)
        {
            if (item.Video != null && item.Video.BitRate != null)
                return $"{item.AwemeId}.{item.Video.BitRate.FirstOrDefault().Format}";
            return $"{item.AwemeId}.mp4";
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
        /// 子类必须实现此方法，可用于更新同步状态、发送通知等
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="syncCount">本次同步成功的视频数量</param>
        /// <returns>一个表示异步操作的任务</returns>
        protected abstract Task HandleSyncCompletion(DouyinCookie cookie, int syncCount);

        /// <summary>
        /// 获取视频实体的差异信息
        /// 子类必须实现此方法，用于区分不同类型的视频（如普通视频、图片合成视频等）
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <returns>视频实体的差异信息，包含视频类型和简化标题</returns>
        protected abstract VideoEntityDifferences GetVideoEntityDifferences(DouyinCookie cookie, Aweme item);

        /// <summary>
        /// 获取NFO文件中的图片（如海报）文件名
        /// 子类可以重写此方法来自定义NFO图片的命名规则
        /// </summary>
        /// <param name="cookie">当前操作的Cookie</param>
        /// <param name="item">视频信息</param>
        /// <param name="config">应用配置</param>
        /// <param name="fileName">原文件名称（如poster.jpg）</param>
        /// <returns>封面图片的文件名</returns>
        protected virtual string GetNfoFileName(DouyinCookie cookie, Aweme item, AppConfig config, string fileName)
        {
            return fileName;
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
                // 检查Cookie是否有效
                if (!IsCookieValid(cookie))
                {
                    Log.Debug($"{JobType}-Cookie[{cookie.UserName}]无效，任务终止!!!");
                    return;
                }
                Log.Debug($"{JobType}- Cookie-[{cookie.UserName}]开始同步...");

                //up主上传视频特殊处理
                if (JobType == SystemStaticUtil.DY_FOLLOWEDS)
                {
                    int syncCount = 0; // 本次同步成功的视频数量
                    string cursor = "0"; // 初始游标
                    bool hasMore = true; // 是否还有更多数据

                    //查询关注列表开启了同步的关注
                    var follows = await douyinFollowService.GetSyncFollows(cookie.MyUserId);

                    var firstUp = follows?.Where(x => !string.IsNullOrWhiteSpace(x.SecUid)).FirstOrDefault();
                    if (firstUp == null)
                    {
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(firstUp.SecUid))
                    {
                        Log.Debug($"{JobType}-Cookie[{firstUp.UperName}]无效,没有sec_userid，任务终止!!!");
                        return;
                    }


                    foreach (var item in follows)
                    {
                        cursor = "0"; // 初始游标
                        await GetViedos(cookie, config, syncCount, cursor, hasMore, item);
                        hasMore = true;
                        // 处理同步完成后的操作
                        await HandleSyncCompletion(cookie, syncCount);
                    }
                }
                else
                {
                    int syncCount = 0; // 本次同步成功的视频数量
                    string cursor = "0"; // 初始游标
                    bool hasMore = true; // 是否还有更多数据
                    (syncCount, cursor, hasMore) = await GetViedos(cookie, config, syncCount, cursor, hasMore);

                    // 处理同步完成后的操作
                    await HandleSyncCompletion(cookie, syncCount);
                }


            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{JobType}-Cookie[{cookie.Id}]同步出错!!!");
            }
        }

        private async Task<(int syncCount, string cursor, bool hasMore)> GetViedos(DouyinCookie cookie, AppConfig config, int syncCount, string cursor, bool hasMore, DouyinFollowed followed = null)
        {
            // 循环获取视频数据
            while (hasMore)
            {
                // 获取视频数据
                var data = await FetchVideoData(cookie, cursor, followed == null ? "" : followed?.SecUid);
                if (data == null)
                {
                    Log.Debug($"{JobType}-Cookie[{cookie.UserName}]读取数据失败!!!");
                    break;
                }

                // 判断是否还有更多数据
                hasMore = ShouldContinueSync(cookie, data, followed);
                // 获取下一页游标
                cursor = GetNextCursor(data);

                // 如果没有视频数据，退出循环
                if (data.AwemeList == null || !data.AwemeList.Any())
                    break;

                // 处理视频列表
                var videos = await ProcessVideoList(cookie, data, config, followed);
                // 保存视频信息到数据库
                syncCount += await SaveVideos(videos);

                //当syncCount达到上限时，跳出循环
                if (config.BatchCount > 0 && syncCount >= config.BatchCount)
                {
                    Log.Debug($"{JobType}-Cookie[{cookie.UserName}]本次同步达到上限{config.BatchCount}，停止同步!!!");
                    break;
                }

                // 随机延迟，模拟人类操作，避免请求过快
                await Task.Delay(_random.Next(5, 10) * 1000);
            }

            return (syncCount, cursor, hasMore);
        }

        /// <summary>
        /// 处理视频列表
        /// 遍历视频列表，分别处理每个视频和图片集
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="data">视频信息对象</param>
        /// <param name="config">应用配置</param>
        /// <param name="followed">关注的</param>
        /// <returns>处理后的视频实体列表</returns>
        protected async Task<List<DouyinVideo>> ProcessVideoList(DouyinCookie cookie, DouyinVideoInfo data, AppConfig config, DouyinFollowed followed = null)
        {
            var videos = new List<DouyinVideo>();
            foreach (var item in data.AwemeList)
            {
                //去重,检查视频是否已存在
                if (config.AutoDistinct)
                {
                    var exitVideo = await douyinVideoService.GetByAwemeId(item.AwemeId);
                    if (exitVideo != null)
                    {
                        if (File.Exists(exitVideo.VideoSavePath))
                        {
                            Log.Debug($"视频-{exitVideo.AwemeId}-[{exitVideo.VideoTitle}]已存在，跳过");
                            continue;// 已存在则跳过
                        }
                    }
                }

                var uper = await douyinFollowService.GetByUperId(item.AuthorUserId.ToString(), cookie.MyUserId);
                if (uper != null)
                {
                    if (uper.FullSync)
                    {
                        followed ??= uper;
                    }
                }
                // 处理单个视频
                var video = await ProcessSingleVideo(cookie, item, data, config, followed);
                if (video != null)
                    videos.Add(video);

                // 如果配置了下载图片视频，则处理图片集并合成视频
                if (config.DownImageVideo || config.DownMp3 || config.DownImage)
                {
                    var mergevideo = await ProcessImageSetAndMergeToVideo(cookie, item, data, config, followed);
                    if (mergevideo != null)
                        videos.Add(mergevideo);
                }
            }
            return videos;
        }

        /// <summary>
        /// 处理单个视频
        /// 负责下载视频、封面、头像，生成NFO文件，创建视频实体等
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <param name="data">视频信息对象</param>
        /// <param name="config">应用配置</param>
        /// <param name="followed">关注</param>
        /// <returns>处理后的视频实体，如果处理失败则为null</returns>
        protected async Task<DouyinVideo> ProcessSingleVideo(DouyinCookie cookie, Aweme item, DouyinVideoInfo data, AppConfig config, DouyinFollowed followed = null)
        {
            // 检查视频数据是否有效
            if (!IsAwemeValid(item)) return null;

            // 获取视频的码率信息
            var v = item.Video.BitRate.Where(x => !string.IsNullOrEmpty(x.Format)).FirstOrDefault();
            if (v == null) return null;

            // 获取视频播放地址
            var videoUrl = v.PlayAddr.UrlList.Where(x => !string.IsNullOrEmpty(x))?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(videoUrl)) return null;

            // 获取视频标签
            var (tag1, tag2, tag3) = GetVideoTags(item);
            // 创建保存文件夹
            var saveFolder = CreateSaveFolder(cookie, item, config, followed);
            // 获取视频文件名
            var fileName = GetVideoFileName(cookie, item, config);
            // 拼接视频保存路径
            var savePath = Path.Combine(saveFolder, fileName);

            // 如果文件已存在，跳过
            if (File.Exists(savePath))
            {
                Log.Debug($"{JobType}-视频[{DouyinFileNameHelper.SanitizePath(item.Desc)}]已存在，跳过下载.");
                return null;
            }

            Log.Debug($"{JobType}-视频[{DouyinFileNameHelper.SanitizePath(item.Desc)}]开始下载...");
            // 随机延迟，模拟人类操作
            await Task.Delay(_random.Next(1, 4) * 1000);
            // 下载视频
            if (!await douyinHttpClientService.DownloadAsync(videoUrl, savePath, cookie.Cookies))
            {
                Log.Error($"{JobType}-{item?.Author?.Nickname ?? ""}-视频[{DouyinFileNameHelper.SanitizePath(item.Desc)}]下载失败!!!");
                return null;
            }
            else
            {
                Log.Debug($"{JobType}-{item?.Author?.Nickname ?? ""}-视频[{DouyinFileNameHelper.SanitizePath(item.Desc)}]下载完成.");
            }

            // 下载视频封面
            await DownVideoCover(item, saveFolder, cookie, config);
            // 下载作者头像
            var (avatarSavePath, avatarUrl) = await DownAuthorAvatar(cookie, item);
            // 生成NFO文件
            await GenerateNfoFile(saveFolder, item, avatarUrl, cookie, config);
            // 创建视频实体
            return CreateVideoEntity(cookie, item, v, savePath, saveFolder, tag1, tag2, tag3, avatarSavePath, avatarUrl, data);
        }

        /// <summary>
        /// 处理图片集并合成为视频
        /// 负责下载图片、合成视频、处理封面和头像等
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息（包含图片集）</param>
        /// <param name="data">视频信息对象</param>
        /// <param name="config">应用配置</param>
        /// <param name="followed">应用配置</param>
        /// <returns>合成后的视频实体，如果处理失败则为null</returns>
        protected async Task<DouyinVideo> ProcessImageSetAndMergeToVideo(DouyinCookie cookie, Aweme item, DouyinVideoInfo data, AppConfig config, DouyinFollowed followed)
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

                if (config.ImageViedoSaveAlone)
                {
                    // 检查图片保存路径是否配置
                    if (string.IsNullOrWhiteSpace(cookie.ImgSavePath))
                    {
                        Log.Error($"{JobType}-图文视频同步-没有配置图片存储路径，任务终止!!!");
                        return null;
                    }
                    fileNamefolder = Path.Combine(cookie.ImgSavePath, DouyinFileNameHelper.GenerateFileName(item.Desc, item.AwemeId));
                }
                else
                {
                    fileNamefolder = CreateSaveFolder(cookie, item, config, followed);
                }
                if (!Directory.Exists(fileNamefolder)) Directory.CreateDirectory(fileNamefolder);

                var fileName = GetVideoFileName(cookie, item, config);
                // 合成视频的保存路径
                var savePath = Path.Combine(fileNamefolder, fileName);

                // 如果文件已存在，返回null
                if (File.Exists(savePath)) return null;

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
                    Log.Error($"{JobType}-图文视频-[{DouyinFileNameHelper.SanitizePath(item.Desc)}]合成失败!!!");
                    return null;
                }

                if (config.DownImageVideo)
                {
                    // 检查合成后的视频文件是否有效
                    if (!File.Exists(savePath) || new FileInfo(savePath).Length <= 0)
                    {
                        Log.Error($"{JobType}-图文视频-[{DouyinFileNameHelper.SanitizePath(item.Desc)}]合成失败!!!");
                        // 清理无效的文件和文件夹
                        if (Directory.Exists(fileNamefolder))
                        {
                            File.Delete(savePath);
                            Directory.Delete(fileNamefolder, true);
                            Log.Error($"{JobType}-图文视频-删除合成失败的视频文件和目录...");
                        }
                        return null;
                    }
                }
                else
                {
                }

                // 下载视频封面（使用第一张图片作为封面）
                await DownVideoCover(imageUrls.FirstOrDefault(), fileNamefolder, cookie, item, config);
                // 下载作者头像
                var (avatarSavePath, avatarUrl) = await DownAuthorAvatar(cookie, item);
                // 生成NFO文件
                await GenerateNfoFile(fileNamefolder, item, avatarUrl, cookie, config);

                // 获取视频标签
                var (tag1, tag2, tag3) = GetVideoTags(item);
                // 为合成的视频创建一个“虚拟”的BitRate对象，以便复用CreateVideoEntity方法
                var virtualBitRate = new VideoBitRate
                {
                    PlayAddr = new PlayAddr
                    {
                        Width = reqParams.VideoWidth,
                        Height = reqParams.VideoHeight,
                        DataSize = new FileInfo(savePath).Length // 合成视频的文件大小
                    }
                };

                // 创建视频实体
                var videoEntity = CreateVideoEntity(
                    cookie, item, virtualBitRate, savePath, fileNamefolder,
                    tag1, tag2, tag3, avatarSavePath, avatarUrl, data);

                // 特殊处理合成视频的字段
                videoEntity.FileHash = string.Empty; // 合成视频没有原始文件哈希
                videoEntity.VideoUrl = "/"; // 合成视频没有原始URL
                videoEntity.ViedoType = VideoTypeEnum.ImageVideo; // 标记为图片合成视频

                return videoEntity;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{JobType}-图片视频同步-处理图片集并合成视频时出错");
                return null;
            }
        }

        /// <summary>
        /// 保存视频信息到数据库
        /// 批量插入视频实体列表到数据库中
        /// </summary>
        /// <param name="videos">要保存的视频实体列表</param>
        /// <returns>保存成功的视频数量</returns>
        protected async Task<int> SaveVideos(List<DouyinVideo> videos)
        {
            if (!videos.Any()) return 0;
            try
            {
                await douyinVideoService.BatchInsertOrUpdate(videos);

                var redowns = await douyinCommonService.GetAllRedown();
                if (redowns != null && redowns.Any())
                {
                    //找出viedos和redowns重复的
                    var duplicateVideos = videos.Where(v => redowns.Any(r => r.ViedoId == v.AwemeId)).ToList();
                    if (duplicateVideos != null && duplicateVideos.Any())
                    {
                        var duplicateVideosIds = duplicateVideos.Select(x => x.AwemeId).ToList();
                        var downeds = redowns.Where(x => duplicateVideosIds.Contains(x.ViedoId))?.ToList();
                        foreach (var item in downeds)
                        {
                            item.Status = 1;
                            var v = videos.FirstOrDefault(x => x.AwemeId == item.ViedoId);
                            Log.Debug($"{JobType}-重新下载成功：-{v.VideoTitle}");
                        }
                        await douyinCommonService.UpdateRedownStatus(downeds);
                    }
                }
                return videos.Count;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{JobType}-批量保存视频到数据库失败");
                // 清理保存失败的视频文件
                await CleanupFailedVideos(videos);
                return 0;
            }
        }

        /// <summary>
        /// 生成NFO文件
        /// NFO文件包含视频的元数据信息，如标题、作者、封面等
        /// </summary>
        /// <param name="saveFolder">NFO文件的保存文件夹</param>
        /// <param name="item">视频信息</param>
        /// <param name="avatarSavePath">作者头像保存路径</param>
        /// <param name="cookie"></param>
        /// <param name="config"></param>
        /// <returns>一个表示异步操作的任务</returns>
        protected async Task GenerateNfoFile(string saveFolder, Aweme item,
            string avatarSavePath, DouyinCookie cookie, AppConfig config)
        {
            // 异步生成NFO文件，避免阻塞主线程
            await Task.Run(() =>
            {
                (string tag1, string tag2, string tag3) = GetVideoTags(item);
                var nfoFileName = GetNfoFileName(cookie, item, config, ".nfo");
                var poster = GetNfoFileName(cookie, item, config, "poster.jpg");
                var nfoPath = Path.Combine(saveFolder, nfoFileName);
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
                    Poster = poster,
                    Title = item.Desc,
                    Thumbnail = poster,// 使用poster作为缩略图
                    ReleaseDate = DateTimeUtil.Convert10BitTimestamp(item.CreateTime),
                    Genres = new List<string> { tag1, tag2, tag3 }.Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                }, nfoPath);
            });
        }

        /// <summary>
        /// 下载视频封面
        /// 从视频信息中提取封面URL并下载到指定文件夹
        /// </summary>
        /// <param name="item">视频信息</param>
        /// <param name="saveFolder">封面保存文件夹</param>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="config">应用配置</param>
        /// <returns>一个表示异步操作的任务</returns>
        protected async Task DownVideoCover(Aweme item, string saveFolder, DouyinCookie cookie, AppConfig config)
        {
            var coverUrl = item.Video.Cover.UrlList?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(coverUrl)) return;
            await DownVideoCover(coverUrl, saveFolder, cookie, item, config);
        }

        /// <summary>
        /// 下载作者头像
        /// 从视频信息中提取作者头像URL并下载到指定文件夹
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <returns>一个元组，包含头像保存路径和头像URL</returns>
        protected async Task<(string savePath, string url)> DownAuthorAvatar(DouyinCookie cookie, Aweme item)
        {
            if (item.Author == null) return (null, null);
            // 优先获取高清头像
            var avatarUrl = item.Author.AvatarLarger?.UrlList?.FirstOrDefault() ?? item.Author.AvatarThumb?.UrlList?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(avatarUrl)) return (null, null);

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
            return (avatarSavePath, avatarUrl);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 检查视频数据是否有效
        /// 验证视频信息是否包含必要的字段
        /// </summary>
        /// <param name="item">视频信息</param>
        /// <returns>如果视频数据有效，则为true；否则为false</returns>
        private bool IsAwemeValid(Aweme item) => item != null && item.Video != null && item.Video.BitRate != null;

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
        /// <param name="videos">保存失败的视频实体列表</param>
        /// <returns>一个表示异步操作的任务</returns>
        private async Task CleanupFailedVideos(List<DouyinVideo> videos)
        {
            Log.Debug($"{JobType}-数据库保存失败，开始清理本次下载的视频目录...");

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
                        Log.Debug($"{JobType}-清理失败视频文件成功: {video.VideoSavePath}!!!");

                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, $"{JobType}-清理失败视频文件出错: {video.VideoSavePath}!!!");
                    }
                });
            }
        }

        /// <summary>
        /// 下载视频封面（重载）
        /// 根据指定的封面URL下载封面图片，并复制为fanart.jpg
        /// </summary>
        /// <param name="coverUrl">封面图片URL</param>
        /// <param name="saveFolder">封面保存文件夹</param>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <param name="config">应用配置</param>
        /// <returns>一个表示异步操作的任务</returns>
        private async Task DownVideoCover(string coverUrl, string saveFolder, DouyinCookie cookie, Aweme item, AppConfig config)
        {
            if (string.IsNullOrWhiteSpace(coverUrl)) return;
            // 获取封面图片文件名
            var coverImgName = GetNfoFileName(cookie, item, config, "poster.jpg");
            var coverSavePath = Path.Combine(saveFolder, coverImgName);

            // 如果封面文件不存在，则下载
            if (!File.Exists(coverSavePath))
            {
                var downRes = await douyinHttpClientService.DownloadAsync(coverUrl, coverSavePath, cookie.Cookies);
            }
        }

        /// <summary>
        /// 创建视频实体
        /// 根据视频信息、下载路径等创建DouyinVideo实体对象
        /// </summary>
        /// <param name="cookie">用户Cookie</param>
        /// <param name="item">视频信息</param>
        /// <param name="bitRate">视频码率信息</param>
        /// <param name="savePath">视频保存路径</param>
        /// <param name="saveFolder">视频保存文件夹</param>
        /// <param name="tag1">视频标签1</param>
        /// <param name="tag2">视频标签2</param>
        /// <param name="tag3">视频标签3</param>
        /// <param name="avatarSavePath">作者头像保存路径</param>
        /// <param name="avatarUrl">作者头像URL</param>
        /// <param name="data">视频信息对象</param>
        /// <returns>创建的视频实体对象</returns>
        private DouyinVideo CreateVideoEntity(
            DouyinCookie cookie, Aweme item, VideoBitRate bitRate, string savePath, string saveFolder,
            string tag1, string tag2, string tag3, string avatarSavePath, string avatarUrl, DouyinVideoInfo data)
        {
            var diffs = GetVideoEntityDifferences(cookie, item);

            return new DouyinVideo
            {
                ViedoType = diffs.VideoType,
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

        #endregion
    }

    /// <summary>
    /// 视频实体差异信息类
    /// 用于存储不同类型视频之间的差异信息
    /// </summary>
    public class VideoEntityDifferences
    {
        /// <summary>
        /// 视频类型
        /// </summary>
        public VideoTypeEnum VideoType { get; set; }

        /// <summary>
        /// 简化的视频标题
        /// </summary>
        public string VideoTitleSimplify { get; set; } = string.Empty;
    }

}