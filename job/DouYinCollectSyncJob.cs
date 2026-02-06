using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.service;
using dy.net.utils;
using System.Threading.Tasks;

namespace dy.net.job
{
    public class DouyinCollectSyncJob : DouyinBasicSyncJob
    {
        public DouyinCollectSyncJob(DouyinCookieService douyinCookieService, DouyinHttpClientService douyinHttpClientService, DouyinVideoService douyinVideoService, DouyinCommonService douyinCommonService, DouyinFollowService douyinFollowService, DouyinMergeVideoService douyinMergeVideoService, DouyinCollectCateService douyinCollectCateService) : base(douyinCookieService, douyinHttpClientService, douyinVideoService, douyinCommonService, douyinFollowService, douyinMergeVideoService, douyinCollectCateService)
        {
        }
        protected override VideoTypeEnum VideoType => VideoTypeEnum.dy_collects;

        protected override async Task BeforeProcessCookies()
        {
            var now = DateTime.Now;
            if (now.Hour == 1 && now.Minute < 30)
            {
                LogFileCleaner.CleanOldLogFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"), 1);
                await Task.Delay(200);
            }
        }


        protected override string GetAuthorAvatarBasePath(DouyinCookie cookie)
        {
            return Path.Combine(cookie.SavePath, "author");
        }

        protected override async Task<DouyinVideoInfoResponse> FetchVideoData(DouyinCookie cookie, string cursor, DouyinFollowed followed, DouyinCollectCate cate)
        {
            return await douyinHttpClientService.SyncCollectVideos(cursor, count, cookie.Cookies);
        }




        protected override string CreateSaveFolder(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed, DouyinCollectCate cate)
        {
            // 1. 简化获取博主自定义保存路径（合并空值判断）
            string saveFolder = !string.IsNullOrWhiteSpace(item?.Author?.Uid)
                ? base.douyinCommonService.GetDouyinUpSavePath(item.Author.Uid)
                : string.Empty;

            // 2. 简化博主文件夹命名逻辑
            string authorFolder = !string.IsNullOrWhiteSpace(saveFolder)
                ? saveFolder
                : (item?.Author == null || (string.IsNullOrWhiteSpace(item.Author.Nickname) && string.IsNullOrWhiteSpace(item.Author.Uid)))
                    ? "未知博主"
                    : DouyinFileNameHelper.SanitizeLinuxFileName(item.Author.Nickname, item.Author.Uid, true);

            if (string.IsNullOrWhiteSpace(saveFolder)&&!string.IsNullOrEmpty(authorFolder))
            {
                base.douyinCommonService.SaveDouyinUpSavePath(item.Author.Uid, authorFolder);
            }

            // 3. 提取重复的视频文件夹名（避免重复调用方法）
            string videoFolderName = DouyinFileNameHelper.SanitizeLinuxFileName(item?.Desc, item?.AwemeId, true);

            // 4. 简化文件夹路径拼接+存在判断（核心逻辑不变）
            string folder = Path.Combine(cookie.SavePath, authorFolder, videoFolderName);
            if (Directory.Exists(folder))
            {
                // 文件夹存在则拼接AwemeId（保留你的原逻辑）
                folder = Path.Combine(cookie.SavePath, authorFolder, $"{videoFolderName}_{item.AwemeId}");
            }
            else
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }
    }
}