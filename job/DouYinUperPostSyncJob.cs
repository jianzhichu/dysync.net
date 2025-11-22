using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace dy.net.job
{
    public class DouyinUperPostSyncJob : DouyinBaseSyncJob
    {
        public DouyinUperPostSyncJob(
             DouyinCookieService dyCookieService,
            DouyinHttpClientService dyHttpClientService,
            DouyinVideoService dyCollectVideoService,
            DouyinCommonService commonService, IServiceProvider serviceProvider, IWebHostEnvironment webHostEnvironment)
            : base(dyCookieService, dyHttpClientService, dyCollectVideoService, commonService, serviceProvider, webHostEnvironment) { }

        protected override string JobType => "dyuploder";

        protected override async Task<List<DouyinUserCookie>> GetValidCookies()
        {
            var cookies = await _dyCookieService.GetAllCookies();
            return cookies.Where(x => !string.IsNullOrWhiteSpace(x.UpSecUserIds) && !string.IsNullOrWhiteSpace(x.UpSavePath)).ToList();
        }

        protected override bool IsCookieValid(DouyinUserCookie cookie)
        {
            return !string.IsNullOrWhiteSpace(cookie.Cookies) && !string.IsNullOrWhiteSpace(cookie.UpSavePath) && !string.IsNullOrWhiteSpace(cookie.UpSecUserIds);
        }

        protected override async Task<DouyinVideoInfo> FetchVideoData(DouyinUserCookie cookie, string cursor)
        {
            // 简化处理：假设只同步第一个UP主
            var ups = JsonConvert.DeserializeObject<List<DouyinUpSecUserIdDto>>(cookie.UpSecUserIds);
            var firstUpId = ups?.FirstOrDefault()?.uid;
            if (string.IsNullOrEmpty(firstUpId)) return null;

            return await _douyinService.SyncUpderPostVideos(count, cursor, firstUpId, cookie.Cookies);
        }

        protected override bool ShouldContinueSync(DouyinUserCookie cookie, DouyinVideoInfo data)
        {
            return data != null && data.HasMore == 1 && cookie.UperSyncd == 0;
        }

        protected override string GetNextCursor(DouyinVideoInfo data)
        {
            return data?.MaxCursor ?? "0";
        }

        protected override string CreateSaveFolder(DouyinUserCookie cookie, Aweme item, string tag1, string tag2)
        {
            // UP主视频通常按作者名创建文件夹
            var authorName = string.IsNullOrWhiteSpace(item.Author?.Nickname) ? "UnknownAuthor" : TikTokFileNameHelper.SanitizePath(item.Author.Nickname);
            var folder = Path.Combine(cookie.UpSavePath, authorName);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        protected override string GetVideoFileName(DouyinUserCookie cookie, Aweme item, VideoBitRate bitRate)
        {
            var config = _commonService.GetConfig();
            if (config?.UperUseViedoTitle ?? false)
            {
                var sampleName = TikTokFileNameHelper.GenerateFileName(item.Desc, item.AwemeId);
                var (existingName, _) = _douyinVideoService.GetUperLastViedoFileName(item.Author.Uid, sampleName).Result;
                return string.IsNullOrWhiteSpace(existingName) ? $"{sampleName}.{bitRate.Format}" : $"{existingName}.{bitRate.Format}";
            }
            return $"{item.AwemeId}.{bitRate.Format}";
        }

        protected override string GetAuthorAvatarBasePath(DouyinUserCookie cookie)
        {
            return Path.Combine(cookie.UpSavePath, "author");
        }

        protected override async Task HandleSyncCompletion(DouyinUserCookie cookie, int syncCount)
        {
            if (syncCount > 0)
            {
                Serilog.Log.Debug($"{JobType}-Cookie-[{cookie.UserName}],本次同步成功{syncCount}条视频");
                cookie.UperSyncd = 1;
                await _dyCookieService.UpdateAsync(cookie);
            }
            else
            {
                Serilog.Log.Debug($"{JobType}-Cookie-[{cookie.UserName}],本次没有查询到新的视频");
            }
        }

        protected override VideoEntityDifferences GetVideoEntityDifferences(DouyinUserCookie cookie, Aweme item)
        {
            var config = _commonService.GetConfig();
            string simplifiedTitle = string.Empty;

            if (config?.UperUseViedoTitle ?? false)
            {
                simplifiedTitle = TikTokFileNameHelper.GenerateFileName(item.Desc, item.AwemeId);
            }

            return new VideoEntityDifferences
            {
                VideoType = VideoTypeEnum.UperPost,
                VideoTitleSimplify = simplifiedTitle
            };
        }
    }
}