using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        protected override string JobType => SystemStaticUtil.DY_FAVORITES;
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

        protected override async Task<DouyinVideoInfo> FetchVideoData(DouyinCookie cookie, string cursor,string uperUid)
        {
            return await douyinHttpClientService.SyncFavoriteVideos(count, cursor, cookie.SecUserId, cookie.Cookies);
        }

        protected override bool ShouldContinueSync(DouyinCookie cookie, DouyinVideoInfo data, DouyinFollowed followed=null)
        {
            return data != null && data.HasMore == 1 && cookie.FavHasSyncd == 0;
        }

        protected override string GetNextCursor(DouyinVideoInfo data)
        {
            return data?.MaxCursor ?? "0";
        }


        protected override string GetAuthorAvatarBasePath(DouyinCookie cookie)
        {
            return Path.Combine(cookie.FavSavePath, "author");
        }

        protected override async Task HandleSyncCompletion(DouyinCookie cookie, int syncCount)
        {
            if (syncCount > 0)
            {
                Serilog.Log.Debug($"{JobType}-Cookie-[{cookie.UserName}],本次同步成功{syncCount}条视频");
                cookie.FavHasSyncd = 1;
                await douyinCookieService.UpdateAsync(cookie);
            }
            else
            {
                Serilog.Log.Debug($"{JobType}-Cookie-[{cookie.UserName}],本次没有查询到新的视频");
            }
        }

        protected override VideoEntityDifferences GetVideoEntityDifferences(DouyinCookie cookie, Aweme item)
        {
            return new VideoEntityDifferences
            {
                VideoType = VideoTypeEnum.Favorite
            };
        }

        protected override string CreateSaveFolder(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed)
        {
            var (tag1, _, _) = GetVideoTags(item);
            var safeTag1 = string.IsNullOrWhiteSpace(tag1) ? "other" : DouyinFileNameHelper.SanitizePath(tag1);
            var folder = Path.Combine(cookie.FavSavePath, safeTag1, $"{DouyinFileNameHelper.SanitizePath(item.Desc)}@{item.AwemeId}");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }
    }
}