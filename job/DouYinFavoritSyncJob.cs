using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.service;
using dy.net.utils;

namespace dy.net.job
{
    public class DouyinFavoritSyncJob : DouyinBasicSyncJob
    {
        public DouyinFavoritSyncJob(
       DouyinCookieService douyinCookieService,
       DouyinHttpClientService douyinHttpClientService,
       DouyinVideoService douyinVideoService,
       DouyinCommonService douyinCommonService,DouyinFollowService douyinFollowService,DouyinMergeVideoService douyinMergeVideoService)
       : base(douyinCookieService, douyinHttpClientService, douyinVideoService, douyinCommonService, douyinFollowService, douyinMergeVideoService) { }


        protected override VideoTypeEnum VideoType => VideoTypeEnum.dy_favorite;

        protected override async Task<List<DouyinCookie>> GetValidCookies()
        {
            return await douyinCookieService.GetAllOpendAsync(x=> !string.IsNullOrWhiteSpace(x.FavSavePath));
        }

        protected override bool IsCookieValid(DouyinCookie cookie)
        {
            return !string.IsNullOrWhiteSpace(cookie.Cookies) && cookie.Cookies.Length >= 1000 &&
                   !string.IsNullOrWhiteSpace(cookie.FavSavePath) &&
                   !string.IsNullOrWhiteSpace(cookie.SecUserId) && cookie.SecUserId.Length >= 10;
        }

        protected override async Task<DouyinVideoInfoResponse> FetchVideoData(DouyinCookie cookie, string cursor,DouyinFollowed followed, DouyinCollectItem collectId)
        {
            return await douyinHttpClientService.SyncFavoriteVideos(count, cursor, cookie.SecUserId, cookie.Cookies);
        }

        protected override bool ShouldContinueSync(DouyinCookie cookie, DouyinVideoInfoResponse data, DouyinFollowed followed=null)
        {
            return data != null && data.HasMore == 1 && cookie.FavHasSyncd == 0;
        }

        protected override string GetNextCursor(DouyinVideoInfoResponse data)
        {
            return data?.MaxCursor ?? "0";
        }


        protected override string GetAuthorAvatarBasePath(DouyinCookie cookie)
        {
            return Path.Combine(cookie.FavSavePath, "author");
        }

        protected override async Task HandleSyncCompletion(DouyinCookie cookie, int syncCount, DouyinFollowed followed)
        {
            if (syncCount > 0)
            {
                Serilog.Log.Debug($"{VideoType}-Cookie-[{cookie.UserName}],本次共同步成功{syncCount}条视频");
                cookie.FavHasSyncd = 1;
                await douyinCookieService.UpdateAsync(cookie);
            }
            else
            {
                Serilog.Log.Debug($"{VideoType}-Cookie-[{cookie.UserName}],没有可以同步的新视频");
            }
        }

        protected override string CreateSaveFolder(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed, DouyinCollectItem collectItem)
        {
            var (tag1, _, _) = GetVideoTags(item);
            var safeTag1 = string.IsNullOrWhiteSpace(tag1) ? "other" : DouyinFileNameHelper.SanitizeLinuxFileName(tag1,"",true);
            var folder = Path.Combine(cookie.FavSavePath, safeTag1, $"{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId,true)}");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }
    }
}