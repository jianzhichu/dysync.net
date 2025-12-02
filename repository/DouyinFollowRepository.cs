using ClockSnowFlake;
using dy.net.dto;
using dy.net.extension;
using dy.net.model;
using SqlSugar;

namespace dy.net.repository
{
    public class DouyinFollowRepository : BaseRepository<DouyinFollowed>
    {
        // 注入SQLSugar客户端
        public DouyinFollowRepository(ISqlSugarClient db) : base(db)
        {
        }



        /// <summary>
        /// 分页查询收藏视频
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>分页结果（视频列表和总数）</returns>
        public async Task<(List<DouyinFollowed> list, int totalCount)> GetPagedAsync(FollowRequestDto dto)
        {
            var where = this.Db.Queryable<DouyinFollowed>()
                .Where(x=>x.mySelfId==dto.MySelfId)
                .WhereIF(!string.IsNullOrWhiteSpace(dto.FollowUserName), x => x.UperName.Contains(dto.FollowUserName));
            var totalCount = await where.CountAsync();
            var list = await where.OrderByDescending(x=>x.OpenSync).OrderByDescending(x => x.LastSyncTime).Skip((dto.PageIndex - 1) * dto.PageSize).Take(dto.PageSize).ToListAsync();
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


        public async Task<DouyinFollowed> GetBySecUId(string uperId,string myId)
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
                .Where(x => x.OpenSync == true).Where(x=>x.mySelfId== userId)
                .ToListAsync();
        }

        /// <summary>
        /// 同步关注列表（新增名字和签名变更检测）
        /// </summary>
        /// <param name="followInfos"></param>
        /// <param name="myselfUserId"></param>
        /// <returns></returns>
        public async Task<bool> Sync(List<FollowingsItem> followInfos, string myselfUserId)
        {
            // 基础参数校验
            if (followInfos == null) followInfos = new List<FollowingsItem>();
            if (string.IsNullOrWhiteSpace(myselfUserId))
            {
                Serilog.Log.Error("同步关注列表失败：当前用户ID为空");
                return false;
            }

            try
            {
                // 1. 查询现有关注列表
                List<DouyinFollowed> existFollows = await Db.Queryable<DouyinFollowed>()
                    .Where(x => x.mySelfId == myselfUserId)
                    .Where(x=>!x.IsNoFollowed) //排除手动添加但未关注的用户
                    .ToListAsync() ?? new List<DouyinFollowed>();

                // 2. 提取现有和当前的SecUid集合（去重优化）
                HashSet<string> existSecUids = existFollows.Select(x => x.SecUid).ToHashSet();
                HashSet<string> currentSecUids = followInfos.Select(x => x.SecUid).ToHashSet();

                // 3. 计算新增、待删除和需要更新的记录
                var toAddFollows = followInfos.Where(x => !existSecUids.Contains(x.SecUid)).ToList();
                var toRemoveFollows = existFollows.Where(x => !currentSecUids.Contains(x.SecUid)).ToList();

                // 3.1 筛选需要更新的记录（SecUid存在但名字或签名有变更）
                var toUpdateFollows = new List<DouyinFollowed>();
                foreach (var existFollow in existFollows)
                {
                    var newFollow = followInfos.FirstOrDefault(x => x.SecUid == existFollow.SecUid);
                    if (newFollow == null) continue;

                    // 检查名字或签名是否变更（精确匹配，区分大小写和空格）
                    bool nameChanged = !string.Equals(existFollow.UperName, newFollow.NickName, StringComparison.Ordinal);
                    bool signatureChanged = !string.Equals(existFollow.Signature, newFollow.Signature, StringComparison.Ordinal);
                    bool enterpriseChanged = !string.Equals(existFollow.Enterprise, newFollow.EnterpriseVerifyReason, StringComparison.Ordinal);
                    bool uperAvatarChanged = !string.Equals(existFollow.UperAvatar, newFollow.Avatar.UrlList?.FirstOrDefault()??"", StringComparison.Ordinal);
                    //bool uperIdChanged = !string.Equals(existFollow.UperId, newFollow.UperId, StringComparison.Ordinal);

                    if (nameChanged || signatureChanged|| uperAvatarChanged|| enterpriseChanged)
                    {
                        // 构造更新实体（仅赋值变更字段和必要字段）
                        var updateEntity = new DouyinFollowed
                        {
                            Id = existFollow.Id, // 主键必须保留，用于匹配
                            mySelfId = existFollow.mySelfId,
                            SecUid = existFollow.SecUid,
                            UperName = newFollow.NickName, // 新名字
                            Signature = newFollow.Signature, // 新签名
                            UperId = newFollow.UperId, // 新的UperId
                            UperAvatar = newFollow.Avatar?.UrlList?.FirstOrDefault() ?? "", // 新头像
                            Enterprise = newFollow.EnterpriseVerifyReason, // 新企业认证
                            LastSyncTime = DateTime.UtcNow // 更新同步时间
                                                           // 其他字段（如Enterprise、UperAvatar、OpenSync）保留原有值，无需赋值
                        };
                        toUpdateFollows.Add(updateEntity);
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
                        mySelfId = myselfUserId,
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
                        return false;
                    }
                }

                // 5. 分批处理更新（适配 SQLSugar 语法：UpdateColumns + WhereColumns）
                if (toUpdateFollows.Any())
                {
                    bool batchUpdateSuccess = await BatchProcessAsync(toUpdateFollows, 200,
                        async batch =>
                        {
                            // SQLSugar 正确用法：实体集合更新 + UpdateColumns（指定更新字段） + WhereColumns（指定匹配字段/主键）
                            int affectedRows = await Db.Updateable(batch) // 传入更新实体集合
                                .UpdateColumns(x => new { x.UperName, x.Signature, x.LastSyncTime,x.Enterprise,x.UperAvatar}) // 仅更新这3个字段
                                .WhereColumns(x => x.Id) // 按主键Id匹配现有记录
                                .ExecuteCommandAsync();

                            // 受影响行数 > 0 或 批次无数据（正常），返回true；否则返回false
                            return affectedRows >= 0;
                        });

                    if (!batchUpdateSuccess)
                    {
                        Serilog.Log.Error("同步关注列表失败：关注信息更新异常");
                        return false;
                    }

                    Serilog.Log.Debug($"同步关注列表：成功更新{toUpdateFollows.Count}条关注信息（用户ID：{myselfUserId}）");
                }

                // 6. 分批处理删除（单批200条）
                if (toRemoveFollows.Any())
                {
                    bool batchDeleteSuccess = await BatchProcessAsync(toRemoveFollows, 200,
                        async batch =>
                        {
                            var secUids = batch.Select(x => x.SecUid).ToList();
                            await DeleteAsync(x => x.mySelfId == myselfUserId && secUids.Contains(x.SecUid));
                            return true;
                        });

                    if (!batchDeleteSuccess)
                    {
                        Serilog.Log.Error("同步关注列表失败：取消关注分批删除异常");
                        return false;
                    }
                }

                Serilog.Log.Debug($"同步关注列表完成（用户ID：{myselfUserId}）：新增{toAddFollows.Count}条，更新{toUpdateFollows.Count}条，删除{toRemoveFollows.Count}条");
                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"同步关注列表失败（用户ID：{myselfUserId}）：{ex.Message}");
                return false;
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
