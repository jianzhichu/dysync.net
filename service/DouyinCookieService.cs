using ClockSnowFlake;
using dy.net.model;
using dy.net.repository;
using SqlSugar;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace dy.net.service
{
    public class DouyinCookieService
    {

        private readonly DouyinCookieRepository _cookieRepository;

        public DouyinCookieService(DouyinCookieRepository cookieRepository)
        {
            _cookieRepository = cookieRepository;
        }

        public Task<List<DouyinCookie>> GetAllOpendAsync(Expression<Func<DouyinCookie, bool>> whereExpression = null)
        {
            return _cookieRepository.GetAllCookies(whereExpression);
        }

        public Task<List<DouyinCookie>> GetAllAsync()
        {
            return _cookieRepository.GetAllAsync();
        }

        public async Task<bool> Add(DouyinCookie dyUserCookies)
        {
            return await _cookieRepository.InsertAsync(dyUserCookies);
        }

        public bool InitCookie()
        {
            var exist = _cookieRepository.GetDefault();
            if (exist != null)
            {
                return true;
            }
            var cookie = new DouyinCookie
            {
                UserName = "douyin",
                Cookies = "-",
                SecUserId = "-",
                Id = IdGener.GetLong().ToString(),
                Status = 0,
                SavePath = "/app/collect",
                FavSavePath = "/app/favorite",
                UpSavePath = "/app/uper",
                ImgSavePath="/app/images",
                CollHasSyncd = 0,
                FavHasSyncd = 0,
                UperSyncd = 0,
                MyUserId=""
            };
            return  _cookieRepository.Insert(cookie);
        }
      
        // 查询单个
        public async Task<DouyinCookie> GetByIdAsync(string id)
        {
            return await _cookieRepository.GetByIdAsync(id);
        }

        // 查询列表（可加条件）
        public async Task<(List<DouyinCookie> list, int totalCount)> GetPagedAsync(int pageIndex, int pageSize)
        {
            return await _cookieRepository.GetPagedAsync(pageIndex, pageSize);
        }

        // 更新
        public async Task<bool> UpdateAsync(DouyinCookie dyUserCookies)
        {
            return await _cookieRepository.UpdateAsync(dyUserCookies);
        }

        // 删除（根据主键）
        public async Task<bool> DeleteByIdAsync(string id)
        {
            return await _cookieRepository.DeleteByIdAsync(id);
        }

        // 批量删除
        public async Task<int> DeleteByIdsAsync(IEnumerable<string> ids)
        {
            return await _cookieRepository.DeleteByIdsAsync(ids.Cast<object>());
        }


        public async Task<bool> ImportCookies(List<DouyinCookie> cookies)
        {
            await _cookieRepository.DeleteAsync(x => !string.IsNullOrWhiteSpace(x.Id));
            await _cookieRepository.InsertRangeAsync(cookies);
            return true;
        }

    }
}
