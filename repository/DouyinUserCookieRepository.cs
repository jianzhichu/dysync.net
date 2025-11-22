using dy.net.model;
using SqlSugar;

namespace dy.net.repository
{
    public class DouyinUserCookieRepository : BaseRepository<DouyinUserCookie>
    {
        // 注入SQLSugar客户端
        public DouyinUserCookieRepository(ISqlSugarClient db) : base(db)
        {
        }

        public async Task<List<DouyinUserCookie>> GetAllCookies()
        {
            return await this.GetListAsync(x=>x.Status==1);
        }


        public async Task<(List<DouyinUserCookie> list, int totalCount)> GetPagedAsync(int pageIndex, int pageSize)
        {
            var where = this.Db.Queryable<DouyinUserCookie>();

            var totalCount = await where.CountAsync();
            var list = await where.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (list, totalCount);
        }

    }
}
