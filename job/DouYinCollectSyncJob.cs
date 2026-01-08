using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
using System;

namespace dy.net.job
{
    public class DouyinCollectSyncJob : DouyinBasicSyncJob
    {
        public DouyinCollectSyncJob(
            DouyinCookieService douyinCookieService,
            DouyinHttpClientService douyinHttpClientService,
            DouyinVideoService douyinVideoService,
            DouyinCommonService douyinCommonService,DouyinFollowService douyinFollowService,DouyinMergeVideoService douyinMergeVideoService)
            : base(douyinCookieService, douyinHttpClientService, douyinVideoService, douyinCommonService,douyinFollowService, douyinMergeVideoService) { }

       
        protected override VideoTypeEnum VideoType => VideoTypeEnum.dy_collects;

        protected override async Task BeforeProcessCookies()
        {
            var now = DateTime.Now;
            if (now.Hour == 1 && now.Minute < 30)
            {
                 //douyinCommonService.UpdateAllCookieSyncedToZero();
                //顺手清理下日志文件
                LogFileCleaner.CleanOldLogFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"), 1);
                await Task.Delay(200); 
            }
        }

        protected override async Task<List<DouyinCookie>> GetValidCookies()
        {
            return await douyinCookieService.GetAllOpendAsync(x => !string.IsNullOrWhiteSpace(x.SavePath));
        }

        protected override bool IsCookieValid(DouyinCookie cookie)
        {
            return !string.IsNullOrWhiteSpace(cookie.Cookies)&& !string.IsNullOrWhiteSpace(cookie.SavePath);
        }

        protected override async Task<DouyinVideoInfo> FetchVideoData(DouyinCookie cookie, string cursor,string uperUid)
        {
            return await douyinHttpClientService.SyncCollectVideos(cursor, count, cookie.Cookies);
        }

        protected override bool ShouldContinueSync(DouyinCookie cookie, DouyinVideoInfo data, DouyinFollowed followed = null)
        {
            return data != null && data.HasMore == 1 && cookie.CollHasSyncd == 0;
        }

        protected override string GetNextCursor(DouyinVideoInfo data)
        {
            return data?.Cursor ?? "0";
        }

        protected override string GetAuthorAvatarBasePath(DouyinCookie cookie)
        {
            return Path.Combine(cookie.SavePath, "author");
        }

        protected override async Task HandleSyncCompletion(DouyinCookie cookie, int syncCount,DouyinFollowed followed)
        {
            if (syncCount > 0)
            {
                Serilog.Log.Debug($"{VideoType}-Cookie-[{cookie.UserName}],本次共同步成功{syncCount}条视频");
                cookie.CollHasSyncd = 1;
                await douyinCookieService.UpdateAsync(cookie);
            }
            else
            {
                Serilog.Log.Debug($"{VideoType}-Cookie-[{cookie.UserName}],没有可以同步的新视频");
            }
        }


        protected override VideoEntityDifferences GetVideoEntityDifferences(DouyinCookie cookie, Aweme item)
        {
            return new VideoEntityDifferences();
        }

        protected override string CreateSaveFolder(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed)
        {
            var (tag1, _, _) = GetVideoTags(item);
            var safeTag1 = string.IsNullOrWhiteSpace(tag1) ? "other" : DouyinFileNameHelper.SanitizeLinuxFileName(tag1,"",true);
            var folder = Path.Combine(cookie.SavePath, safeTag1, $"{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId,true)}");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }
    }
}