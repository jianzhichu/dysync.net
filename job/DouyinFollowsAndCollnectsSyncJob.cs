using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.service;
using Quartz;

namespace dy.net.job
{
    [DisallowConcurrentExecution] // 禁止并发执行，确保同一时间只有一个实例在运行
    public class DouyinFollowsAndCollnectsSyncJob : IJob
    {


        /// <summary>
        /// 抖音收藏夹服务
        /// </summary>
        protected readonly DouyinCollectCateService douyinCollectCateService;
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

        public DouyinFollowsAndCollnectsSyncJob(DouyinCookieService dyCookieService, DouyinHttpClientService douyinService, DouyinFollowService followService, DouyinCommonService douyinCommonService, DouyinCollectCateService douyinCollectCateService)
        {
            _dyCookieService = dyCookieService;
            _douyinService = douyinService;
            _followService = followService;
            this.douyinCommonService = douyinCommonService;
            this.douyinCollectCateService = douyinCollectCateService;
        }

        // 第一步：定义常量和枚举（建议放在单独的常量类中，此处为内联演示）
        private const string DEFAULT_FOLLOW_COUNT = "20";
        private const int INVALID_COOKIE_STATUS_CODE = 8;
        private const string LOG_TAG_COLLECT = "收藏夹同步";
        private const string LOG_TAG_MIX = "合集列表同步";
        private const string LOG_TAG_SERIES = "短剧列表同步";
        private const string LOG_TAG_FOLLOW = "关注列表同步";

        /// <summary>
        /// 抖音数据同步任务执行方法（优化版）
        /// </summary>
        /// <param name="context">任务执行上下文</param>
        public async Task Execute(IJobExecutionContext context)
        {
            // 1. 获取有效Cookie列表，提前做空值判断
            var cookies = await _dyCookieService.GetOpendCookiesAsync();
            if (cookies == null || !cookies.Any())
            {
                Serilog.Log.Debug("当前无可用的抖音Cookie，同步任务跳过");
                return;
            }

            // 2. 获取应用配置
            AppConfig conf = douyinCommonService.GetConfig();

            // 3. 遍历每个Cookie，执行各类数据同步
            foreach (var ck in cookies)
            {
                Serilog.Log.Information($"开始处理Cookie用户：[{ck.UserName}]（ID：{ck.Id}）");

                // 3.1 同步自定义收藏夹（UseCollectFolder=true时）
                if (ck.UseCollectFolder)
                {
                    await SyncCollectDataAsync(
                        cookie: ck,
                        cateType: VideoTypeEnum.dy_custom_collect,
                        dataFetchFunc: async (offset) => await _douyinService.SyncCollectFolderList(ck.Cookies, offset),
                        entityConvertFunc: (collectItem) => new DouyinCollectCate
                        {
                            CookieId = ck.Id,
                            CateType = VideoTypeEnum.dy_custom_collect,
                            Name = collectItem.CollectsName,
                            Sync = false,
                            XId = collectItem.CollectsId
                        },
                        hasMoreFunc: (data) => data?.HasMore ?? false,
                        cursorFunc: (data) => data?.Cursor.ToString() ?? "0",
                        dataListFunc: (data) => data?.CollectsList,
                        logTag: LOG_TAG_COLLECT
                    );
                }

                // 3.2 同步收藏夹合集（DownMix=true时）
                if (ck.DownMix)
                {
                    await SyncCollectDataAsync(
                        cookie: ck,
                        cateType: VideoTypeEnum.dy_mix,
                        dataFetchFunc: async (offset) => await _douyinService.SyncMixList(ck.Cookies, offset),
                        entityConvertFunc: (mixItem) => new DouyinCollectCate
                        {
                            CookieId = ck.Id,
                            CateType = VideoTypeEnum.dy_mix,
                            Name = mixItem.MixName,
                            Sync = false,
                            XId = mixItem.MixId,
                            CoverUrl = mixItem.CoverUrl?.UrlList?.FirstOrDefault() // 空值防护：避免CoverUrl为null
                        },
                        hasMoreFunc: (data) => (data?.HasMore ?? 0) == 1,
                        cursorFunc: (data) => data?.Cursor.ToString() ?? "0",
                        dataListFunc: (data) => data?.MixInfos,
                        logTag: LOG_TAG_MIX
                    );
                }

                // 3.3 同步收藏夹短剧（DownSeries=true时）
                if (ck.DownSeries)
                {
                    await SyncCollectDataAsync(
                        cookie: ck,
                        cateType: VideoTypeEnum.dy_series,
                        dataFetchFunc: async (offset) => await _douyinService.SyncSeriesList(ck.Cookies, offset),
                        entityConvertFunc: (seriesItem) => new DouyinCollectCate
                        {
                            CookieId = ck.Id,
                            CateType = VideoTypeEnum.dy_series,
                            Name = seriesItem.SeriesName,
                            Sync = false,
                            XId = seriesItem.SeriesId,
                            CoverUrl = seriesItem.CoverImage?.ImageUrlList?.FirstOrDefault() // 空值防护：避免CoverImage为null
                        },
                        hasMoreFunc: (data) => (data?.HasMore ?? 0) == 1,
                        cursorFunc: (data) => data?.Cursor.ToString() ?? "0",
                        dataListFunc: (data) => data?.SeriesList,
                        logTag: LOG_TAG_SERIES
                    );
                }

                // 3.4 同步关注列表（单独处理，逻辑特殊不纳入通用方法）
                await SyncFollowListAsync(ck, conf);

                Serilog.Log.Debug($"完成Cookie用户：[{ck.UserName}]（ID：{ck.Id}）的所有收藏夹信息、合集信息、关注列表信息、短剧信息同步");
            }

            // 4. 标记首次运行完成（仅执行一次）
            await douyinCommonService.SetConfigNotFirstRunning();
        }

        #region 私有通用辅助方法
        /// <summary>
        /// 通用收藏类数据同步方法（泛型封装，消除冗余）
        /// </summary>
        /// <typeparam name="TData">抖音返回的分页数据类型</typeparam>
        /// <typeparam name="TItem">抖音返回的列表项类型</typeparam>
        /// <param name="cookie">当前Cookie信息</param>
        /// <param name="cateType">分类类型</param>
        /// <param name="dataFetchFunc">分页数据获取委托</param>
        /// <param name="entityConvertFunc">抖音项转换为DouyinCollectCate的委托</param>
        /// <param name="hasMoreFunc">判断是否还有更多数据的委托</param>
        /// <param name="cursorFunc">获取下一页游标值的委托</param>
        /// <param name="dataListFunc">从分页数据中提取列表的委托</param>
        /// <param name="logTag">日志标签</param>
        private async Task SyncCollectDataAsync<TData, TItem>(
            DouyinCookie cookie,
            VideoTypeEnum cateType,
            Func<string, Task<TData>> dataFetchFunc,
            Func<TItem, DouyinCollectCate> entityConvertFunc,
            Func<TData, bool> hasMoreFunc,
            Func<TData, string> cursorFunc,
            Func<TData, List<TItem>> dataListFunc,
            string logTag)
        {
            string offset = "0";
            bool hasMore = true;
            List<DouyinCollectCate> collectDataList = new List<DouyinCollectCate>();

            try
            {
                // 1. 分页获取抖音数据
                while (hasMore)
                {
                    var pageData = await dataFetchFunc(offset);
                    if (pageData == null)
                    {
                        //Serilog.Log.Warning($"[{cookie.UserName}] - {logTag}：获取分页数据为空，停止分页");
                        break;
                    }

                    // 2. 更新分页状态
                    hasMore = hasMoreFunc(pageData);
                    offset = cursorFunc(pageData);

                    // 3. 提取当前页列表数据并转换
                    var currentPageItems = dataListFunc(pageData);
                    if (currentPageItems != null && currentPageItems.Any())
                    {
                        var convertItems = currentPageItems.Select(entityConvertFunc).ToList();
                        collectDataList.AddRange(convertItems);
                        Serilog.Log.Debug($"[{cookie.UserName}] - {logTag}：获取到{currentPageItems.Count}条数据，累计{collectDataList.Count}条");
                    }
                }

                // 4. 调用Sync方法同步到数据库
                if (collectDataList.Any())
                {
                    var (add, update, delete, succ) = await douyinCollectCateService.Sync(
                        collectDataList,
                        cookie.Id,
                        cateType);

                    Serilog.Log.Debug($"[{cookie.UserName}] - {logTag}：同步完成，新增{add}条，更新{update}条，删除{delete}条，是否成功：{succ}");
                }
                else
                {
                    Serilog.Log.Debug($"[{cookie.UserName}] - {logTag}：无有效数据需要同步");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"[{cookie.UserName}] - {logTag}：分页同步失败，异常信息：{ex.Message}");
            }
        }

        /// <summary>
        /// 关注列表单独同步方法（逻辑特殊，单独封装）
        /// </summary>
        /// <param name="cookie">当前Cookie信息</param>
        /// <param name="config">应用配置</param>
        private async Task SyncFollowListAsync(DouyinCookie cookie, AppConfig config)
        {
            // 1. 校验SecUserId是否有效
            if (string.IsNullOrWhiteSpace(cookie.SecUserId))
            {
                Serilog.Log.Debug($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：未设置SecUserId，跳过");
                return;
            }

            string offset = "0";
            bool hasMore = true;
            int total = 0;
            List<FollowingsItem> followList = new List<FollowingsItem>();

            try
            {
                // 2. 分页获取关注列表数据
                while (hasMore)
                {
                    var (data, error) = await FetchFollowPageDataAsync(cookie, offset);

                    // 3. 处理错误信息
                    if (error != null && error.StatusCode != 0)
                    {
                        Serilog.Log.Error($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：发生错误，状态码：{error.StatusCode}，错误信息：{error.StatusMsg}");
                        // 无效Cookie（未登录）直接停止分页
                        if (error.StatusCode == INVALID_COOKIE_STATUS_CODE)
                        {
                            cookie.StatusMsg = "无效";
                            cookie.StatusCode = INVALID_COOKIE_STATUS_CODE;
                            await _dyCookieService.UpdateAsync(cookie);
                            break;
                        }
                    }

                    // 4. 处理有效数据
                    if (data != null)
                    {
                        // 5. 更新MyUserId（若未设置）
                        if (string.IsNullOrWhiteSpace(cookie.MyUserId))
                        {
                            cookie.MyUserId = data.MySelfUserId;
                            await _dyCookieService.UpdateAsync(cookie);
                        }

                        // 6. 更新分页状态和数据
                        total = data.Total;
                        hasMore = data.HasMore;
                        offset = data.Offset.ToString();

                        if (data.Followings != null && data.Followings.Any())
                        {
                            followList.AddRange(data.Followings);
                            Serilog.Log.Debug($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：获取到{data.Followings.Count}条关注数据，累计{followList.Count}条");
                        }

                        // 7. 非首次运行时，仅同步第一页数据
                        if (!config.IsFirstRunning)
                        {
                            hasMore = false;
                            //Serilog.Log.Debug($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：非首次运行，停止分页获取");
                        }
                    }
                    else
                    {
                        Serilog.Log.Warning($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：获取分页数据为空，停止分页");
                        break;
                    }
                }

                // 8. 同步关注列表到数据库
                if (followList.Any())
                {
                    var (add, update, succ) = await _followService.Sync(followList, cookie);
                    Serilog.Log.Information($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：同步完成，新增{add}条，更新{update}条，是否成功：{succ}，总关注数：{total}");
                }
                else
                {
                    Serilog.Log.Debug($"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：无有效关注数据需要同步");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"[{cookie.UserName}] - {LOG_TAG_FOLLOW}：同步失败，异常信息：{ex.Message}");
            }
        }

        /// <summary>
        /// 分页获取关注列表数据（封装回调逻辑，简化代码）
        /// </summary>
        /// <param name="cookie">当前Cookie信息</param>
        /// <param name="offset">分页偏移量</param>
        private async Task<(DouyinFollowInfoResponse Data, FollowErrorDto Error)> FetchFollowPageDataAsync(DouyinCookie cookie, string offset)
        {
            DouyinFollowInfoResponse resultData = null;
            FollowErrorDto resultError = null;
            resultData = await _douyinService.SyncMyFollows(
                DEFAULT_FOLLOW_COUNT,
                offset,
                cookie.SecUserId,
                cookie.Cookies,
                async (err) =>
                {
                    resultError = err;
                    // 更新Cookie状态（异步不阻塞）
                    cookie.StatusMsg = err.StatusCode == INVALID_COOKIE_STATUS_CODE ? "无效" : "正常";
                    cookie.StatusCode = err.StatusCode;
                    await _dyCookieService.UpdateAsync(cookie);
                });

            // 若原方法是回调式无返回值，需调整封装逻辑
            return (resultData, resultError);
        }
        #endregion
    }
}
