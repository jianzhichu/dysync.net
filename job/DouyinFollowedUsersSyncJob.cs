using dy.net.dto;
using dy.net.model;
using dy.net.service;
using Quartz;
using System.Collections.Generic;

namespace dy.net.job
{
    [DisallowConcurrentExecution] // 禁止并发执行，确保同一时间只有一个实例在运行
    public class DouyinFollowedUsersSyncJob : IJob
    {


        /// <summary>
        /// 抖音Cookie服务，用于获取和管理用户Cookie
        /// </summary>
        protected readonly DouyinCookieService _dyCookieService;

        /// <summary>
        /// 抖音HTTP客户端服务，用于发送HTTP请求
        /// </summary>
        protected readonly DouyinHttpClientService _douyinService;

        /// <summary>
        /// 抖音关注服务，用于管理关注数据
        /// </summary>
        protected readonly DouyinFollowService _followService;

        public DouyinFollowedUsersSyncJob(DouyinCookieService dyCookieService, DouyinHttpClientService douyinService, DouyinFollowService followService)
        {
            _dyCookieService = dyCookieService;
            _douyinService = douyinService;
            _followService = followService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var cookies = await _dyCookieService.GetAllOpendAsync();
            if (cookies != null && cookies.Any())
            {

                foreach (var ck in cookies)
                {
                    string count = "20";
                    string offset = "0";
                    bool hasmore = true;
                    int total= 0;
                    List<FollowingsItem> follows = new List<FollowingsItem>();
                    while (hasmore)
                    {
                        var data = await _douyinService.SyncMyFollows(count, offset, ck.SecUserId, ck.Cookies);

                        if (data != null)
                        {
                            // 绑定我的用户ID--之前没有这个字段
                            if (string.IsNullOrWhiteSpace(ck.MyUserId))
                            {
                                ck.MyUserId = data.MySelfUserId;
                                await _dyCookieService.UpdateAsync(ck);
                            }
                            total = data.Total;
                            hasmore = data.HasMore;
                            offset = data.Offset.ToString();
                            if (data.Followings != null && data.Followings.Count > 0)
                            {
                                follows.AddRange(data.Followings);
                            }
                        }
                    }
                  
                    if (follows.Count > 0)
                    {
                        await _followService.Sync(follows, ck.MyUserId);
                    }

                    Serilog.Log.Debug($"当前Cookie-[{ck.UserName}]，本次同步关注列表完成，共同步关注{total}人。");
                }
            }
        }
    }
}
