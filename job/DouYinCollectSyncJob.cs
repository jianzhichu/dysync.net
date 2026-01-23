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

        //protected override bool ShouldContinueSync(DouyinCookie cookie, DouyinVideoInfoResponse data, DouyinFollowed followed , AppConfig config)
        //{
        //    return data != null && data.HasMore == 1 && cookie.CollHasSyncd == 0;
        //}

        //protected override async Task HandleSyncCompletion(DouyinCookie cookie, int syncCount,DouyinFollowed followed,DouyinCollectCate cate)
        //{
        //    cookie.CollHasSyncd = 1;
        //    await douyinCookieService.UpdateAsync(cookie);
        //    Log.Debug($"[{VideoType}]-[{cookie.UserName}],本次成功同步{syncCount}条视频");
        //}



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
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;

        }
    }
}