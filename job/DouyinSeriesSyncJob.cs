using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.service;
using dy.net.utils;
using System.Net;

namespace dy.net.job
{
    public class DouyinSeriesSyncJob : DouyinBasicSyncJob
    {
        public DouyinSeriesSyncJob(DouyinCookieService douyinCookieService, DouyinHttpClientService douyinHttpClientService, DouyinVideoService douyinVideoService, DouyinCommonService douyinCommonService, DouyinFollowService douyinFollowService, DouyinMergeVideoService douyinMergeVideoService, DouyinCollectCateService douyinCollectCateService) : base(douyinCookieService, douyinHttpClientService, douyinVideoService, douyinCommonService, douyinFollowService, douyinMergeVideoService, douyinCollectCateService)
        {
        }


        protected override VideoTypeEnum VideoType => VideoTypeEnum.dy_series;

        protected override async Task<DouyinVideoInfoResponse> FetchVideoData(DouyinCookie cookie, string cursor,DouyinFollowed followed, DouyinCollectCate cate)
        {
            return await douyinHttpClientService.SyncSeriesViedosByMSeriesId(cursor, count, cookie.Cookies,cate.XId);
        }

        protected override bool ShouldContinueSync(DouyinCookie cookie, DouyinVideoInfoResponse data, DouyinFollowed followed=null)
        {
            return data != null && data.HasMore == 1;
        }
    }
}