using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.service;
using dy.net.utils;

namespace dy.net.job
{
    public class DouyinFavoritSyncJob : DouyinBasicSyncJob
    {
        public DouyinFavoritSyncJob(DouyinCookieService douyinCookieService, DouyinHttpClientService douyinHttpClientService, DouyinVideoService douyinVideoService, DouyinCommonService douyinCommonService, DouyinFollowService douyinFollowService, DouyinMergeVideoService douyinMergeVideoService, DouyinCollectCateService douyinCollectCateService) : base(douyinCookieService, douyinHttpClientService, douyinVideoService, douyinCommonService, douyinFollowService, douyinMergeVideoService, douyinCollectCateService)
        {
        }


        protected override VideoTypeEnum VideoType => VideoTypeEnum.dy_favorite;

        protected override string GetAuthorAvatarBasePath(DouyinCookie cookie)
        {
            return Path.Combine(cookie.FavSavePath, "author");
        }

        protected override async Task<List<DouyinCookie>> GetSyncCookies()
        {
            return await douyinCookieService.GetOpendCookiesAsync(x => !string.IsNullOrWhiteSpace(x.FavSavePath) && !string.IsNullOrWhiteSpace(x.SecUserId));
        }

        protected override async Task<DouyinVideoInfoResponse> FetchVideoData(DouyinCookie cookie, string cursor, DouyinFollowed followed, DouyinCollectCate cate)
        {
            return await douyinHttpClientService.SyncFavoriteVideos(count, cursor, cookie.SecUserId, cookie.Cookies);
        }

        //protected override bool ShouldContinueSync(DouyinCookie cookie, DouyinVideoInfoResponse data, DouyinFollowed followed, AppConfig config)
        //{
        //    return data != null && data.HasMore == 1;
        //}


        //protected override async Task HandleSyncCompletion(DouyinCookie cookie, int syncCount, DouyinFollowed followed,DouyinCollectCate cate)
        //{
        //    cookie.FavHasSyncd = 1;
        //    await douyinCookieService.UpdateAsync(cookie);
        //    await base.HandleSyncCompletion(cookie, syncCount, followed, cate);
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
            var folder = Path.Combine(cookie.FavSavePath, authorFolder, $"{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId, true)}");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                //说明文件夹存在，检查里面有没有文件，如果已经有视频文件了，说明视频标题相同，那么应该重新创建文件夹,+id

                folder = Path.Combine(cookie.SavePath, authorFolder, $"{DouyinFileNameHelper.SanitizeLinuxFileName(item.Desc, item.AwemeId, true)}" + "_" + item.AwemeId);
            }
            return folder;
        }
    }
}