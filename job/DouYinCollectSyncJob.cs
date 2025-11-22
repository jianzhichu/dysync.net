using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;

namespace dy.net.job
{
    public class DouyinCollectSyncJob : DouyinBaseSyncJob
    {
        public DouyinCollectSyncJob(
            DouyinCookieService dyCookieService,
            DouyinHttpClientService dyHttpClientService,
            DouyinVideoService dyCollectVideoService,
            DouyinCommonService commonService,IServiceProvider serviceProvider,IWebHostEnvironment webHostEnvironment)
            : base(dyCookieService, dyHttpClientService, dyCollectVideoService, commonService, serviceProvider,webHostEnvironment) { }

        protected override string JobType => "collect";

        protected override async Task BeforeProcessCookies()
        {
            var now = DateTime.Now;
            if (now.Hour == 1 && now.Minute < 30)
            {
                 _commonService.UpdateAllCookieSyncedToZero();
                await Task.Delay(200); 
            }
        }

        protected override async Task<List<DouyinUserCookie>> GetValidCookies()
        {
            var cookies = await _dyCookieService.GetAllCookies();
            return cookies.Where(c => !string.IsNullOrWhiteSpace(c.SavePath)).ToList();
        }

        protected override bool IsCookieValid(DouyinUserCookie cookie)
        {
            return !string.IsNullOrWhiteSpace(cookie.Cookies) && cookie.Cookies.Length >= 1000 &&
                   !string.IsNullOrWhiteSpace(cookie.SavePath);
        }

        protected override async Task<DouyinVideoInfo> FetchVideoData(DouyinUserCookie cookie, string cursor)
        {
            return await _douyinService.SyncCollectVideos(cursor, count, cookie.Cookies);
        }

        protected override bool ShouldContinueSync(DouyinUserCookie cookie, DouyinVideoInfo data)
        {
            return data != null && data.HasMore == 1 && cookie.CollHasSyncd == 0;
        }

        protected override string GetNextCursor(DouyinVideoInfo data)
        {
            return data?.Cursor ?? "0";
        }

        protected override string CreateSaveFolder(DouyinUserCookie cookie, Aweme item, string tag1, string tag2)
        {
            var safeTag1 = string.IsNullOrWhiteSpace(tag1) ? "other" : TikTokFileNameHelper.SanitizePath(tag1);
            var folder = Path.Combine(cookie.SavePath, safeTag1, $"{TikTokFileNameHelper.SanitizePath(item.Desc)}@{item.AwemeId}");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        protected override string GetVideoFileName(DouyinUserCookie cookie, Aweme item, VideoBitRate bitRate)
        {
            return $"{item.AwemeId}.{bitRate.Format}";
        }

        protected override string GetAuthorAvatarBasePath(DouyinUserCookie cookie)
        {
            return Path.Combine(cookie.SavePath, "author");
        }

        protected override async Task HandleSyncCompletion(DouyinUserCookie cookie, int syncCount)
        {
            if (syncCount > 0)
            {
                Serilog.Log.Debug($"{JobType}-Cookie-[{cookie.UserName}],本次同步成功{syncCount}条视频");
                cookie.CollHasSyncd = 1;
                await _dyCookieService.UpdateAsync(cookie);
            }
            else
            {
                Serilog.Log.Debug($"{JobType}-Cookie-[{cookie.UserName}],本次没有查询到新的视频");
            }
        }


        protected override VideoEntityDifferences GetVideoEntityDifferences(DouyinUserCookie cookie, Aweme item)
        {
            return new VideoEntityDifferences
            {
                VideoType = VideoTypeEnum.Collect,
            };
        }
    }
}