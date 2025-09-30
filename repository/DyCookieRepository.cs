using dy.net.model;
using SqlSugar;

namespace dy.net.repository
{
    public class DyCookieRepository : BaseRepository<DyUserCookies>
    {
        // 注入SQLSugar客户端
        public DyCookieRepository(ISqlSugarClient db) : base(db)
        {
        }

        public async Task<List<DyUserCookies>> GetAllCookies()
        {
            return await this.GetListAsync(x=>x.Status==1);
        }


        public async Task<(List<DyUserCookies> list, int totalCount)> GetPagedAsync(int pageIndex, int pageSize)
        {
            var where = this.Db.Queryable<DyUserCookies>();

            var totalCount = await where.CountAsync();
            var list = await where.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (list, totalCount);
        }

    }
}
