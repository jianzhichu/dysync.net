using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.service;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dy.net.job
{
    [DisallowConcurrentExecution]
    public class DouyinFollowsAndCollnectsSyncJob : IJob
    {
        // 服务依赖（统一下划线命名）
        private readonly DouyinCollectCateService _douyinCollectCateService;
        private readonly DouyinCookieService _dyCookieService;
        private readonly DouyinHttpClientService _douyinService;
        private readonly DouyinFollowService _followService;
        private readonly DouyinCommonService _douyinCommonService;

        // 常量定义
        private const string DEFAULT_FOLLOW_COUNT = "20";
        private const int INVALID_COOKIE_STATUS_CODE = 8;
        private const string LOG_TAG_COLLECT = "收藏列表";
        private const string LOG_TAG_MIX = "合集列表";
        private const string LOG_TAG_SERIES = "短剧列表";
        private const string LOG_TAG_FOLLOW = "关注列表";

        // 构造函数注入
        public DouyinFollowsAndCollnectsSyncJob(
            DouyinCookieService dyCookieService,
            DouyinHttpClientService douyinService,
            DouyinFollowService followService,
            DouyinCommonService douyinCommonService,
            DouyinCollectCateService douyinCollectCateService)
        {
            _dyCookieService = dyCookieService;
            _douyinService = douyinService;
            _followService = followService;
            _douyinCommonService = douyinCommonService;
            _douyinCollectCateService = douyinCollectCateService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var cookies = await _dyCookieService.GetOpendCookiesAsync();
            if (cookies == null || !cookies.Any())
            {
                Log.Debug("当前无可用的抖音Cookie，同步任务跳过");
                return;
            }

            var conf = _douyinCommonService.GetConfig();

            foreach (var ck in cookies)
            {
                Log.Information($"开始处理Cookie用户：[{ck.UserName}]（ID：{ck.Id}）");

                // 调用通用方法，传入差异化参数（无委托，参数直观）
                if (ck.UseCollectFolder)
                {
                    await SyncCollectGenericAsync(ck, VideoTypeEnum.dy_custom_collect,
                        (cookie, offset) => _douyinService.SyncCollectFolderList(cookie.Cookies, offset),
                        item => new DouyinCollectCate
                        {
                            CookieId = ck.Id,
                            CateType = VideoTypeEnum.dy_custom_collect,
                            Name = item.CollectsName,
                            Sync = false,
                            XId = item.CollectsId,
                            Total = item.TotalNumber
                        },
                        data => data?.HasMore ?? false,
                        data => data?.Cursor.ToString() ?? "0",
                        data => data?.CollectsList,
                        LOG_TAG_COLLECT);
                }

                if (ck.DownMix)
                {
                    await SyncCollectGenericAsync(ck, VideoTypeEnum.dy_mix,
                        (cookie, offset) => _douyinService.SyncMixList(cookie.Cookies, offset),
                        item => new DouyinCollectCate
                        {
                            CookieId = ck.Id,
                            CateType = VideoTypeEnum.dy_mix,
                            Name = item.MixName,
                            Sync = false,
                            XId = item.MixId,
                            Total = item?.Statis?.UpdatedToEpisode ?? 0,
                            CoverUrl = item.CoverUrl?.UrlList?.LastOrDefault()
                        },
                        data => (data?.HasMore ?? 0) == 1,
                        data => data?.Cursor.ToString() ?? "0",
                        data => data?.MixInfos,
                        LOG_TAG_MIX);
                }

                if (ck.DownSeries)
                {
                    await SyncCollectGenericAsync(ck, VideoTypeEnum.dy_series,
                        (cookie, offset) => _douyinService.SyncSeriesList(cookie.Cookies, offset),
                        item => new DouyinCollectCate
                        {
                            CookieId = ck.Id,
                            CateType = VideoTypeEnum.dy_series,
                            Name = item.SeriesName,
                            Sync = false,
                            XId = item.SeriesId,
                            Total = item?.Stats?.TotalEpisodeCount ?? 0,
                            CoverUrl = item.CoverImage?.ImageUrlList?.FirstOrDefault()
                        },
                        data => (data?.HasMore ?? 0) == 1,
                        data => data?.Cursor.ToString() ?? "0",
                        data => data?.SeriesList,
                        LOG_TAG_SERIES);
                }

                await SyncFollowListAsync(ck, conf);
                Log.Debug($"完成[{ck.UserName}] [列表]同步， 包括 [自定义收藏夹、关注、合集、短剧] ");
            }

        }

        #region 通用核心方法（无冗余，支持调试）
        /// <summary>
        /// 通用收藏类数据同步方法（无复杂委托，参数直观，便于调试）
        /// </summary>
        /// <typeparam name="TData">分页数据类型</typeparam>
        /// <typeparam name="TItem">列表项类型</typeparam>
        private async Task SyncCollectGenericAsync<TData, TItem>(
            DouyinCookie cookie,
            VideoTypeEnum cateType,
            // 数据获取方法（最简委托，仅传必要参数）
            Func<DouyinCookie, string, Task<TData>> dataFetchFunc,
            // 实体转换方法（内联lambda，调试可直接看到转换逻辑）
            Func<TItem, DouyinCollectCate> entityConvertFunc,
            // 分页判断方法
            Func<TData, bool> hasMoreFunc,
            // 游标获取方法
            Func<TData, string> getCursorFunc,
            // 列表提取方法
            Func<TData, List<TItem>> getDataListFunc,
            string logTag)
        {
            var collectDataList = new List<DouyinCollectCate>();
            string offset = "0";
            bool hasMore = true;

            try
            {
                while (hasMore)
                {
                    // 1. 获取分页数据（直接调用，调试可断点到具体Service方法）
                    var pageData = await dataFetchFunc(cookie, offset);
                    if (pageData == null) break;

                    // 2. 更新分页状态（逻辑透明）
                    hasMore = hasMoreFunc(pageData);
                    offset = getCursorFunc(pageData);

                    // 3. 提取并转换数据（内联逻辑，调试可看每一项转换结果）
                    var currentItems = getDataListFunc(pageData);
                    if (currentItems?.Any() == true)
                    {
                        collectDataList.AddRange(currentItems.Select(entityConvertFunc));
                        Log.Debug($"[{cookie.UserName}] - {logTag}：获取{currentItems.Count}条，累计{collectDataList.Count}条");
                    }
                }

                // 4. 同步到数据库
                if (collectDataList.Any())
                {
                    var (add, update, delete, succ) = await _douyinCollectCateService.Sync(collectDataList, cookie.Id, cateType);
                    Log.Debug($"[{cookie.UserName}] - {logTag}：同步完成 新增:{add} 更新:{update} 删除:{delete} 成功:{succ}");
                }
                else
                {
                    Log.Debug($"[{cookie.UserName}] - {logTag}：无有效数据");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{cookie.UserName}] - {logTag}：同步失败");
            }
        }
        #endregion

        #region 关注列表同步（逻辑独立，无重复）
        private async Task SyncFollowListAsync(DouyinCookie cookie, AppConfig config)
        {
            if (string.IsNullOrWhiteSpace(cookie.SecUserId))
            {
                Log.Debug($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：未设置SecUserId，跳过");
                return;
            }

            var followList = new List<FollowingsItem>();
            string offset = "0";
            bool hasMore = true;
            int total = 0;
            FollowErrorDto currentError = null;

            try
            {
                while (hasMore)
                {
                    var data = await _douyinService.SyncMyFollows(
                        DEFAULT_FOLLOW_COUNT, offset, cookie.SecUserId, cookie.Cookies,
                        async (err) =>
                        {
                            currentError = err;
                            cookie.StatusMsg = err.StatusCode == INVALID_COOKIE_STATUS_CODE ? "无效" : "正常";
                            cookie.StatusCode = err.StatusCode;
                            await _dyCookieService.UpdateAsync(cookie);
                        });

                    if (currentError != null && currentError.StatusCode != 0)
                    {
                        Log.Error($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：错误 状态码:{currentError.StatusCode} 信息:{currentError.StatusMsg}");
                        if (currentError.StatusCode == INVALID_COOKIE_STATUS_CODE) break;
                    }

                    if (data == null) break;

                    if (string.IsNullOrWhiteSpace(cookie.MyUserId))
                    {
                        cookie.MyUserId = data.MySelfUserId;
                        await _dyCookieService.UpdateAsync(cookie);
                    }

                    total = data.Total;
                    hasMore = data.HasMore;
                    offset = data.Offset.ToString();

                    if (data.Followings?.Any() == true)
                    {
                        followList.AddRange(data.Followings);
                        //Log.Debug($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：获取{data.Followings.Count}条，累计{followList.Count}条");
                    }

                    if (!config.IsFirstRunning) hasMore = false;
                }

                if (followList.Any())
                {
                    var (add, update, succ) = await _followService.Sync(followList, cookie);
                    Log.Information($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：同步完成 新增:{add} 更新:{update} 成功:{succ} 总关注数:{total}");
                }
                await _douyinCommonService.SetConfigNotFirstRunning();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：同步失败");
            }
        }
        #endregion
    }
}