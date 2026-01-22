using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.service;
using dy.net.utils;

namespace dy.net.job
{
    public class DouyinMixSyncJob : DouyinBasicSyncJob
    {
        public DouyinMixSyncJob(DouyinCookieService douyinCookieService, DouyinHttpClientService douyinHttpClientService, DouyinVideoService douyinVideoService, DouyinCommonService douyinCommonService, DouyinFollowService douyinFollowService, DouyinMergeVideoService douyinMergeVideoService, DouyinCollectCateService douyinCollectCateService) : base(douyinCookieService, douyinHttpClientService, douyinVideoService, douyinCommonService, douyinFollowService, douyinMergeVideoService, douyinCollectCateService)
        {
        }


        protected override VideoTypeEnum VideoType => VideoTypeEnum.dy_mix;


        protected override async Task<DouyinVideoInfoResponse> FetchVideoData(DouyinCookie cookie, string cursor,DouyinFollowed followed,DouyinCollectCate cate)
        {
            return await douyinHttpClientService.SyncMixViedosByMixId(cursor,count,cookie.Cookies,cate.XId);
        }

        //protected override bool ShouldContinueSync(DouyinCookie cookie, DouyinVideoInfoResponse data, DouyinFollowed followed=null)
        //{
        //    return data != null && data.HasMore == 1;
        //}
        protected override string CreateSaveFolder(DouyinCookie cookie, Aweme item, AppConfig config, DouyinFollowed followed, DouyinCollectCate cate)
        {
            if (cate != null)
            {
                var folder = Path.Combine(cookie.SavePath,VideoType.GetDesc(), DouyinFileNameHelper.SanitizeLinuxFileName(cate.SaveFolder, cate.Name, true));
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                return folder;
            }
            else
            {
                return base.CreateSaveFolder(cookie, item, config, followed, cate);
            }
        }

        protected override string GetAuthorAvatarBasePath(DouyinCookie cookie)
        {
            return Path.Combine(cookie.SavePath, "author");
        }
    }
}