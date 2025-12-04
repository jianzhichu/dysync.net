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

        protected readonly DouyinCommonService douyinCommonService;

        public DouyinFollowedUsersSyncJob(DouyinCookieService dyCookieService, DouyinHttpClientService douyinService, DouyinFollowService followService, DouyinCommonService douyinCommonService)
        {
            _dyCookieService = dyCookieService;
            _douyinService = douyinService;
            _followService = followService;
            this.douyinCommonService = douyinCommonService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var cookies = await _dyCookieService.GetAllOpendAsync();
            if (cookies != null && cookies.Any())
            {
                AppConfig conf = douyinCommonService.GetConfig();

                foreach (var ck in cookies)
                {
                    string count = "20";
                    string offset = "0";
                    bool hasmore = true;
                    int total= 0;
                    List<FollowingsItem> follows = new List<FollowingsItem>();
                    while (hasmore)
                    {
                        try
                        {
                            var data = await _douyinService.SyncMyFollows(count, offset, ck.SecUserId, ck.Cookies, async err =>
                            {
                                if (err.StatusCode != 0)
                                {
                                    Serilog.Log.Error($"同步用户[{ck.UserName}]关注列表时发生错误，错误信息：{err.StatusMsg}，状态码：{err.StatusCode}");
                                }
                                // 如果是未登录错误，则跳出循环
                                if (err.StatusCode == 8)
                                {
                                    hasmore = false;
                                }
                                ck.StatusMsg = err.StatusCode == 8 ? "已过期" : "正 常";
                                ck.StatusCode = err.StatusCode;
                                await _dyCookieService.UpdateAsync(ck);
                            });

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

                            // 只有第一次启动时，才进行全部同步
                            if (!conf.IsFirstRunning)
                            {
                                hasmore = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Serilog.Log.Error($"关注列表同步失败,{ex.Message}");
                            hasmore = false;
                            break;
                        }
                    }
                  
                    if (follows.Count > 0)
                    {
                        await _followService.Sync(follows, ck );
                    }

                }
              await  douyinCommonService.SetConfigNotFirstRunning();
            }
        }
    }
}
