using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using SqlSugar;

namespace dy.net.repository
{
    public class DouyinFollowRepository : BaseRepository<DouyinFollowed>
    {
        // 注入SQLSugar客户端
        public DouyinFollowRepository(ISqlSugarClient db) : base(db)
        {
        }


        public async Task<List<DouyinFollowGroupDto>> GetDouyinFollowGroup()
        {
            var data = this.Db.Queryable<DouyinFollowed>()
                        .LeftJoin<DouyinCookie>((f, u) => f.mySelfId == u.MyUserId)
                        //.Where((f, u) => u.Status == 1) // 注意：LeftJoin+u.Status==1 等价于 InnerJoin（u必须存在）
                        .GroupBy((f, u) => f.mySelfId) // 按 mySelfId 分组
                        .Select((f, u) => new DouyinFollowGroupDto
                        {
                            Key = f.mySelfId, // 分组键（mySelfId）
                            Name = u.UserName, // 取每组的 UserName（因分组唯一，用 First() 兼容 SQL）
                            Total = SqlFunc.AggregateCount(f.Id) // 统计每组的记录数（用主表主键避免 null 影响）
                        })
                        .ToList(); // 执行查询

            return data;
        }

        /// <summary>
        /// 分页查询收藏视频
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>分页结果（视频列表和总数）</returns>
        public async Task<(List<DouyinFollowed> list, int totalCount)> GetPagedAsync(FollowRequestDto dto)
        {
            var where = this.Db.Queryable<DouyinFollowed>()
                .Where(x => x.mySelfId == dto.MySelfId)
                .WhereIF(!string.IsNullOrWhiteSpace(dto.FollowUserName), x => x.UperName.Contains(dto.FollowUserName));
            var totalCount = await where.CountAsync();
            var list = await where.OrderByDescending(x => x.OpenSync).OrderByDescending(x => x.LastSyncTime).Skip((dto.PageIndex - 1) * dto.PageSize).Take(dto.PageSize).ToListAsync();
            return (list, totalCount);
        }


        public async Task<bool> BatchInsert(List<DouyinFollowed> followeds)
        {
            return await Db.Insertable(followeds).ExecuteCommandAsync() > 0;
        }



        public async Task<bool> BatchUpdate(List<DouyinFollowed> followeds)
        {
            return await Db.Updateable(followeds).ExecuteCommandAsync() > 0;
        }


        public async Task<DouyinFollowed> GetBySecUId(string secUid)
        {
            return await this.GetFirstAsync(x => x.SecUid == secUid);
        }


        public async Task<DouyinFollowed> GetBySecUId(string uperId, string myId)
        {
            return await this.GetFirstAsync(x => x.UperId == uperId && x.mySelfId == myId);
        }
        public async Task<bool> Update(DouyinFollowed followed)
        {
            return await this.UpdateAsync(followed);
        }

        public async Task<bool> Insert(DouyinFollowed followed)
        {
            return await this.InsertAsync(followed);
        }

        public async Task<List<DouyinFollowed>> GetSyncFollows(string userId)
        {
            return await this.Db.Queryable<DouyinFollowed>()
                .Where(x => x.OpenSync == true).Where(x => x.mySelfId == userId).Where(x => !string.IsNullOrWhiteSpace(x.SecUid))
                .ToListAsync();
        }


        /// <summary>
        /// 同步关注列表（新增名字和签名变更检测）
        /// </summary>
        /// <param name="followInfos"></param>
        /// <param name="ck"></param>
        /// <returns></returns>
        public async Task<(int add, int update, bool succ)> Sync(List<FollowingsItem> followInfos, DouyinCookie ck)
        {
            // 基础参数校验
            if (followInfos == null) followInfos = new List<FollowingsItem>();
            if (ck == null || string.IsNullOrWhiteSpace(ck.MyUserId))
            {
                Serilog.Log.Error("同步关注列表失败：当前用户ID为空");
                return (0, 0, false);
            }

            try
            {
                // 2. 开启SqlSugar事务
                await Db.Ado.BeginTranAsync();
                // 1. 提取当前批次的SecUid集合（去重）
                HashSet<string> currentSecUids = followInfos.Select(x => x.SecUid).ToHashSet();
                if (!currentSecUids.Any())
                {
                    Serilog.Log.Debug($"同步关注列表：当前批次无有效数据（{ck.UserName}），直接返回成功");
                    return (0, 0, true);
                }

                // 2. 查询当前批次对应的现有记录（仅查需要对比的，减少数据量）
                List<DouyinFollowed> existFollows = await Db.Queryable<DouyinFollowed>()
                    .Where(x => x.mySelfId == ck.MyUserId)
                    .Where(x => !x.IsNoFollowed)
                    .Where(x => currentSecUids.Contains(x.SecUid)) // 仅查当前批次的SecUid
                    .ToListAsync() ?? new List<DouyinFollowed>();

                // 3. 拆分：新增（当前批次有，数据库无） + 更新（当前批次有，数据库也有且字段变化）
                HashSet<string> existSecUids = existFollows.Select(x => x.SecUid).ToHashSet();
                var toAddFollows = followInfos.Where(x => !existSecUids.Contains(x.SecUid)).ToList();
                var toUpdateFollows = new List<DouyinFollowed>();

                // 3.1 筛选需要更新的记录
                foreach (var existFollow in existFollows)
                {
                    var newFollow = followInfos.FirstOrDefault(x => x.SecUid == existFollow.SecUid);
                    if (newFollow == null) continue;

                    // 检查字段是否变更（精确匹配）
                    bool nameChanged = !string.Equals(existFollow.UperName, newFollow.NickName, StringComparison.Ordinal);
                    bool signatureChanged = !string.Equals(existFollow.Signature, newFollow.Signature, StringComparison.Ordinal);
                    bool enterpriseChanged = !string.Equals(existFollow.Enterprise, newFollow.EnterpriseVerifyReason, StringComparison.Ordinal);
                    bool uperAvatarChanged = !string.Equals(existFollow.UperAvatar, newFollow.Avatar?.UrlList?.FirstOrDefault() ?? "", StringComparison.Ordinal);

                    if (nameChanged || signatureChanged || uperAvatarChanged || enterpriseChanged)
                    {
                        toUpdateFollows.Add(new DouyinFollowed
                        {
                            Id = existFollow.Id, // 主键用于匹配
                            mySelfId = ck.MyUserId,
                            SecUid = existFollow.SecUid,
                            UperName = newFollow.NickName,
                            Signature = newFollow.Signature,
                            Enterprise = newFollow.EnterpriseVerifyReason,
                            UperAvatar = newFollow.Avatar?.UrlList?.FirstOrDefault() ?? "",
                            LastSyncTime = DateTime.UtcNow // 更新同步时间
                        });
                    }
                }

                // 4. 分批处理新增（单批200条）
                if (toAddFollows.Any())
                {
                    Func<FollowingsItem, DouyinFollowed> mapToDouyinFollowed = follow => new DouyinFollowed
                    {
                        Id = IdGener.GetLong().ToString(),
                        Enterprise = follow.EnterpriseVerifyReason,
                        LastSyncTime = DateTime.UtcNow,
                        mySelfId = ck.MyUserId,
                        SecUid = follow.SecUid,
                        OpenSync = false,
                        UperAvatar = follow.Avatar?.UrlList?.FirstOrDefault() ?? "",
                        UperName = follow.NickName,
                        Signature = follow.Signature,
                        UperId = follow.UperId
                    };

                    bool batchAddSuccess = await BatchProcessAsync(toAddFollows, 200,
                        async batch => await BatchInsert(batch.Select(mapToDouyinFollowed).ToList()));

                    if (!batchAddSuccess)
                    {
                        Serilog.Log.Error("同步关注列表失败：新增关注分批插入异常");
                        return (toAddFollows.Count, toUpdateFollows.Count, false);
                    }
                }

                // 5. 分批处理更新（适配SQLSugar语法）
                if (toUpdateFollows.Any())
                {
                    bool batchUpdateSuccess = await BatchProcessAsync(toUpdateFollows, 200,
                        async batch =>
                        {
                            int affectedRows = await Db.Updateable(batch)
                                .UpdateColumns(x => new { x.UperName, x.Signature, x.LastSyncTime, x.Enterprise, x.UperAvatar })
                                .WhereColumns(x => x.Id) // 按主键匹配
                                .ExecuteCommandAsync();

                            return affectedRows >= 0;
                        });

                    if (!batchUpdateSuccess)
                    {
                        Serilog.Log.Error( $"[{ck.UserName}]同步关注列表失败");
                        return (toAddFollows.Count, toUpdateFollows.Count, false);
                    }
                }

                // 6. 提交事务
                await Db.Ado.CommitTranAsync();
                // 【重要】删除逻辑已移除：增量场景下不能通过批次对比删除，需单独设计取消关注逻辑
                //Serilog.Log.Debug($"[{ck.UserName}]关注列表同步完成：新增{toAddFollows.Count}条，更新{toUpdateFollows.Count}条");
                return (toAddFollows.Count, toUpdateFollows.Count, true);
            }
            catch (Exception ex)
            {
                await Db.Ado.RollbackTranAsync();
                Serilog.Log.Error(ex, $"[{ck.UserName}]同步关注列表失败：{ex.Message}");
                return (0, 0, false);

            }
        }


        /// <summary>
        /// 通用分批处理工具方法
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="dataList">待处理数据</param>
        /// <param name="batchSize">单批大小</param>
        /// <param name="processAction">单批处理逻辑（返回是否成功）</param>
        /// <returns>整体处理结果</returns>
        private async Task<bool> BatchProcessAsync<T>(List<T> dataList, int batchSize, Func<List<T>, Task<bool>> processAction)
        {
            if (dataList == null || !dataList.Any() || batchSize <= 0)
                return true;

            int totalCount = dataList.Count;
            int batchCount = (int)Math.Ceiling((double)totalCount / batchSize);

            for (int i = 0; i < batchCount; i++)
            {
                var batch = dataList.Skip(i * batchSize).Take(batchSize).ToList();
                if (!batch.Any()) continue;

                bool success = await processAction(batch);
                if (!success)
                {
                    Serilog.Log.Debug($"分批处理失败：第{i + 1}批（数据范围：{i * batchSize}-{Math.Min((i + 1) * batchSize - 1, totalCount - 1)}）");
                    return false;
                }
            }

            return true;
        }

    }
}
