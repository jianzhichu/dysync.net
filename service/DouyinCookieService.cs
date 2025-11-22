using dy.net.model;
using dy.net.repository;
using SqlSugar;
using System.Linq.Expressions;

namespace dy.net.service
{
    public class DouyinCookieService
    {

        private readonly DouyinUserCookieRepository _cookieRepository;

        public DouyinCookieService(DouyinUserCookieRepository cookieRepository)
        {
            _cookieRepository = cookieRepository;
        }

        public Task<List<DouyinUserCookie>> GetAllCookies()
        {
            return _cookieRepository.GetAllCookies();
        }


        public async Task<bool> Add(DouyinUserCookie dyUserCookies)
        {
            return await _cookieRepository.InsertAsync(dyUserCookies);
        }

        public bool InitCookie()
        {
            var initId= "2026";
            var exist = _cookieRepository.GetFirst(x => x.Id == initId);
            if (exist != null)
            {
                return false;
            }
            var cookie = new DouyinUserCookie
            {
                UserName = "douyin",
                Cookies = "--",
                Id = initId,
                SavePath = "/app/collect",
                Status = 0,
                SecUserId = "--",
                FavSavePath = "/app/favorite",
                UpSavePath = "/app/uper",
                CollHasSyncd = 0,
                FavHasSyncd = 0,
                UperSyncd = 0,
                ImgSavePath="/app/images",
            };
            return  _cookieRepository.Insert(cookie);
        }
      
        // 查询单个
        public async Task<DouyinUserCookie> GetByIdAsync(string id)
        {
            return await _cookieRepository.GetByIdAsync(id);
        }

        // 查询列表（可加条件）
        public async Task<(List<DouyinUserCookie> list, int totalCount)> GetPagedAsync(int pageIndex, int pageSize)
        {
            return await _cookieRepository.GetPagedAsync(pageIndex, pageSize);
        }

        // 更新
        public async Task<bool> UpdateAsync(DouyinUserCookie dyUserCookies)
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


    }
}
