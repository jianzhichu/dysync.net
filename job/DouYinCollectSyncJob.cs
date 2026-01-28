using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.service;
using dy.net.utils;

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
            string authorFolder;
            if (string.IsNullOrWhiteSpace(item.Author?.Nickname) && string.IsNullOrWhiteSpace(item.Author?.Uid))
            {
                authorFolder = "未知博主";
            }
            else
            {
                authorFolder = $"{DouyinFileNameHelper.SanitizeLinuxFileName(item.Author?.Nickname, item.Author?.Uid, true)}";
            }
            var folder = Path.Combine(cookie.SavePath, authorFolder, $"{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId, true)}");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                //说明文件夹存在，检查里面有没有文件，如果已经有视频文件了，说明视频标题相同，那么应该重新创建文件夹,+id

                folder = Path.Combine(cookie.SavePath, authorFolder , $"{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId, true)}"+"_" + item.AwemeId);
            }
            return folder;

        }
    }
}