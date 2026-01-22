using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.utils;
using SqlSugar;
using System;
using System.Linq.Expressions;

namespace dy.net.repository
{
    public class DouyinCollectCateRepository : BaseRepository<DouyinCollectCate>
    {
        // 注入SQLSugar客户端
        public DouyinCollectCateRepository(ISqlSugarClient db) : base(db)
        {
        }


        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<(List<DouyinCollectCate> list, int totalCount)> GetPagedAsync(DouyinCollectCateRequestDto dto)
        {
            var where = this.Db.Queryable<DouyinCollectCate>()
                .Where(x=>x.CateType==dto.cateType)
                .WhereIF(!string.IsNullOrWhiteSpace(dto.cookieId), x => x.CookieId == dto.cookieId);

            var totalCount = await where.CountAsync();
            var list = await where.Skip((dto.PageIndex - 1) * dto.PageSize).Take(dto.PageSize).ToListAsync();
            return (list, totalCount);
        }

        /// <summary>
        /// 批量修改状态
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> SwitchBatchAsync(List<DouyinCollectCateSwitchDto> dto)
        {
            // 空值/空列表防护 + 提取ID列表
            var ids = dto?.Select(x => x.Id).ToList() ?? new List<string>();
            if (!ids.Any()) return false;

            // 1. 查询需要更新的分类数据
            var cates = await GetListAsync(x => ids.Contains(x.Id));

            // 2. 用字典优化查找（避免循环内FirstOrDefault，提升性能）
            var dtoDict = dto.ToDictionary(x => x.Id);

            // 3. 批量更新状态（简化循环写法）
            cates.ForEach(item =>
            {
                if (dtoDict.TryGetValue(item.Id, out var dtocate))
                {
                    item.Sync = dtocate.Sync;
                    item.SaveFolder = string.IsNullOrWhiteSpace(dtocate.Folder) ? DouyinFileNameHelper.SanitizeLinuxFileName(item.Name, item.Id, true) : dtocate.Folder;
                    item.UpdateTime = DateTime.Now;
                }
            });

            // 4. 执行更新并返回结果
            return await Db.Updateable(cates).ExecuteCommandAsync() > 0;
        }

        internal async Task<(int add, int update, int delete, bool succ)> Sync(List<DouyinCollectCate> cates, string ckId, VideoTypeEnum cateType)
        {
            // 初始化返回结果
            int addCount = 0;
            int updateCount = 0;
            int deleteCount = 0;
            bool isSuccess = false;

            try
            {
                // 1. 参数校验
                if (string.IsNullOrWhiteSpace(ckId))
                {
                    throw new ArgumentNullException(nameof(ckId), "CookieId信息不能为空");
                }
                if (cates == null)
                {
                    cates = new List<DouyinCollectCate>(); // 空列表则执行删除所有操作
                }

                // 2. 开启SqlSugar事务
                await Db.Ado.BeginTranAsync();

                // 3. 查询数据库中当前用户的所有分类，并转为Dictionary优化查找效率（核心优化1）
                var dbCates = await Db.Queryable<DouyinCollectCate>()
                    .Where(c => c.CookieId == ckId && c.CateType == cateType)
                    .ToListAsync();
                // 以XId为Key构建字典，避免循环内FirstOrDefault的低效查找
                var dbCateDict = dbCates.ToDictionary(c => c.XId, c => c);

                // 4. 处理新增和【批量】更新逻辑
                var xIds = cates.Select(c => c.XId).ToList();
                // 定义批量更新的待处理集合（核心优化2）
                var toUpdateCates = new List<DouyinCollectCate>();

                foreach (var cate in cates)
                {
                    // 用Dictionary查找，效率更高
                    if (dbCateDict.TryGetValue(cate.XId, out var existCate))
                    {
                        // 对比字段是否有变化
                        bool isChanged = !string.Equals(existCate.Name, cate.Name, StringComparison.Ordinal) ||
                            !string.Equals(existCate.CoverUrl, cate.CoverUrl, StringComparison.Ordinal) ||
                            !string.Equals(existCate.SaveFolder, cate.SaveFolder, StringComparison.Ordinal) ||
                            existCate.Total != cate.Total;

                        if (isChanged)
                        {
                            // 补充更新信息，加入批量更新集合
                            cate.UpdateTime = DateTime.Now;
                            // 确保更新时的Id正确（从数据库已有记录中获取）
                            cate.Id = existCate.Id;
                            toUpdateCates.Add(cate);
                        }
                    }
                    else
                    {
                        // 新增逻辑保持不变
                        cate.CreateTime = DateTime.Now;
                        cate.Id = IdGener.GetLong().ToString();
                        await Db.Insertable(cate).ExecuteCommandAsync();
                        addCount++;
                    }
                }

                // 5. 执行批量更新（核心优化3：批量提交，减少数据库IO）
                if (toUpdateCates.Any())
                {
                    updateCount = await Db.Updateable(toUpdateCates)
                        .IgnoreColumns(x => new { x.Sync, x.CreateTime, x.CookieId })
                        .IgnoreNullColumns() // 忽略空值字段
                        .ExecuteCommandAsync();
                }

                // 6. 处理删除逻辑：数据库中有但传入列表中没有的分类
                var deleteCates = dbCates.Where(c => !xIds.Contains(c.XId)).ToList();
                if (deleteCates.Any())
                {
                    var deleteIds = deleteCates.Select(c => c.Id).ToList();
                    deleteCount = await Db.Deleteable<DouyinCollectCate>()
                        .Where(c => deleteIds.Contains(c.Id) && c.CookieId == ckId)
                        .ExecuteCommandAsync();
                }

                // 7. 提交事务
                await Db.Ado.CommitTranAsync();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                // 异常时回滚事务（补充：避免Db.Ado未初始化事务导致的空引用）
                await Db.Ado?.RollbackTranAsync();
                isSuccess = false;
                Serilog.Log.Error($"同步{cateType.GetDesc()}时发生异常，{ex.StackTrace}");
            }

            //Serilog.Log.Debug($"本次收藏夹{cateType}同步结果,新增:{addCount},更新:{updateCount},删除:{deleteCount}");
            // 8. 返回最终结果
            return (addCount, updateCount, deleteCount, isSuccess);
        }

        /// <summary>
        /// 短剧、合集完结
        /// </summary>
        /// <param name="cate"></param>
        /// <returns></returns>
        public async Task<bool> UpdateCateEndStatus(DouyinCollectCate cate)
        {
            var videoCounts = await Db.Queryable<DouyinVideo>().Where(x => x.CateId == cate.Id && x.CateXId == cate.XId && x.ViedoType == cate.CateType).CountAsync();
            cate.IsEnd = videoCounts == cate.Total;
         
            var update = await Db.Updateable(cate).UpdateColumns(x => new { x.IsEnd }).ExecuteCommandAsync();
            return update > 0;
        }
    }
}
