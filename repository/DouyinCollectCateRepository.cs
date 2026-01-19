using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.model.entity;
using SqlSugar;
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
                }
            });

            // 4. 执行更新并返回结果
            return await Db.Updateable(cates).ExecuteCommandAsync() > 0;
        }

        internal async Task<(int add, int update, int delete, bool succ)> Sync(List<DouyinCollectCate> cates, string ckId,int cateType)
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

                // 3. 查询数据库中当前用户的所有分类
                var dbCates = await Db.Queryable<DouyinCollectCate>()
                    .Where(c => c.CookieId == ckId && c.CateType == cateType)
                    .ToListAsync();

                // 4. 处理新增和更新逻辑
                var cateIds = cates.Select(c => c.Id).ToList();
                foreach (var cate in cates)
                {

                    cate.UpdateTime = DateTime.Now; // 更新时间戳

                    // 检查是否已存在
                    if (string.IsNullOrWhiteSpace(cate.Id))
                    {
                        cate.CreateTime = DateTime.Now;
                        cate.Id = IdGener.GetLong().ToString();
                        // 新增
                        await Db.Insertable(cate).ExecuteCommandAsync();
                        addCount++;
                    }
                    else
                    {
                        var existCate = dbCates.FirstOrDefault(x => x.Id == cate.Id);
                        if (existCate == null)
                        {
                            Serilog.Log.Error($" cate {cate.Id} id 不存在");
                            continue;
                        }
                        // 对比字段是否有变化（根据实际业务字段调整）
                        bool isChanged = !string.Equals(existCate.Name, cate.Name, StringComparison.Ordinal);

                        if (isChanged)
                        {
                            // 更新（只更新有变化的字段，或全量更新）
                            await Db.Updateable(cate)
                                .IgnoreColumns(ignoreAllNullColumns: true) // 忽略空值字段
                                .Where(c => c.Id == cate.Id)
                                .ExecuteCommandAsync();
                            updateCount++;
                        }
                    }
                }

                // 5. 处理删除逻辑：数据库中有但传入列表中没有的分类
                var deleteCates = dbCates.Where(c => !cateIds.Contains(c.Id)).ToList();
                if (deleteCates.Any())
                {
                    var deleteIds = deleteCates.Select(c => c.Id).ToList();
                    deleteCount = await Db.Deleteable<DouyinCollectCate>()
                        .Where(c => deleteIds.Contains(c.Id) && c.CookieId == ckId)
                        .ExecuteCommandAsync();
                }

                // 6. 提交事务
                await Db.Ado.CommitTranAsync();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                // 异常时回滚事务
                await Db.Ado.RollbackTranAsync();
                isSuccess = false;
                // throw;
                Serilog.Log.Error($"同步收藏夹:类型-{cateType}异常，{ex.StackTrace}" );
            }

            // 7. 返回最终结果
            return (addCount, updateCount, deleteCount, isSuccess);
        }
    }
}
