using dy.net.model;
using SqlSugar;
using System.Linq.Expressions;

namespace dy.net.repository
{
    public class DouyinCookieRepository : BaseRepository<DouyinCookie>
    {
        // 注入SQLSugar客户端
        public DouyinCookieRepository(ISqlSugarClient db) : base(db)
        {
        }

        public async Task<List<DouyinCookie>> GetAllCookies(Expression<Func<DouyinCookie, bool>> whereExpression = null)
        {
            // 1. 初始化查询：先加固定条件 Status == 1
            var query = Db.Queryable<DouyinCookie>()
                          .Where(x => x.Status == 1); // 固定条件（必选）

            // 2. 若传入自定义条件，叠加 Where（自动 AND 组合）
            if (whereExpression != null)
            {
                query = query.Where(whereExpression); // 自定义条件（可选）
            }

            // 3. 执行查询（SqlSugar 自动合并所有 Where 条件）
            return await query.ToListAsync();
        }

        public async Task<(List<DouyinCookie> list, int totalCount)> GetPagedAsync(int pageIndex, int pageSize)
        {
            var where = this.Db.Queryable<DouyinCookie>();

            var totalCount = await where.CountAsync();
            var list = await where.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (list, totalCount);
        }

        public  DouyinCookie GetDefault()
        {
            return  Db.Queryable<DouyinCookie>()
                                .First();
        }

    }
}
