using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.repository;
using System.Linq.Expressions;

namespace dy.net.service
{
    public class DouyinCookieService
    {

        private readonly DouyinCookieRepository _cookieRepository;

        public DouyinCookieService(DouyinCookieRepository cookieRepository)
        {
            _cookieRepository = cookieRepository;
        }

        public Task<List<DouyinCookie>> GetOpendCookiesAsync(Expression<Func<DouyinCookie, bool>> whereExpression = null)
        {
            return _cookieRepository.GetAllCookiesAsync(whereExpression);
        }


        public async Task<List<DouyinCookie>> GetOpendCookies()
        {
            return await _cookieRepository.GetAllCookies();
        }
        public Task<List<DouyinCookie>> GetAllAsync()
        {
            return _cookieRepository.GetAllAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsInit()
        {
            return await _cookieRepository.ExistsAsync(x => !string.IsNullOrEmpty(x.Id));
        }
        public async Task<bool> Add(DouyinCookie dyUserCookies)
        {
            return await _cookieRepository.InsertAsync(dyUserCookies);
        }
        public async Task<bool> Switch(DouyinCookieSwitchDto dto)
        {
            return await _cookieRepository.SwitchAsync(dto);
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
                UserName = "douyin2026",
                Cookies = "",
                SecUserId = "",
                Id = IdGener.GetLong().ToString(),
                Status = 0,
                SavePath = "/app/collect",
                FavSavePath = "/app/favorite",
                UpSavePath = "/app/uper",
                //ImgSavePath="/app/images",
                //CollHasSyncd = 0,
                //FavHasSyncd = 0,
                //UperSyncd = 0,
                MyUserId = ""
            };
            return _cookieRepository.Insert(cookie);
        }

        /// <summary>
        /// 新增字段，兼容旧版本
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateCookieToSupportOldVersionAsync()
        {
            var cookies = _cookieRepository.GetAll();

            foreach (var item in cookies)
            {
                item.DownCollect = !string.IsNullOrWhiteSpace(item.SavePath) && !item.UseCollectFolder;
                item.DownFavorite = !string.IsNullOrWhiteSpace(item.FavSavePath);
                item.DownFollowd = !string.IsNullOrWhiteSpace(item.UpSavePath);
            }
            await _cookieRepository.UpdateRangeAsync(cookies);
            return true;
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

        // 更新
        public async Task<bool> UpdateCookieAsync(DouyinCookie dyUserCookies)
        {
            return await _cookieRepository.UpdateCookie(dyUserCookies);
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

        /// <summary>
        /// 将所有同步类型的同步状态改为已同步，这样，之后就不会再扫描所有接口数据，只会读取最新一页的数据了
        /// </summary>
        /// <returns></returns>
        //public async Task<bool> SetOnlySyncNew()
        //{
        //    var cks= await _cookieRepository.GetAllAsync();

        //    foreach (var item in cks)
        //    {
        //        item.CollHasSyncd = 1;
        //        item.FavHasSyncd = 1;
        //        item.UperSyncd = 1;
        //    }
        //   var d= await _cookieRepository.UpdateRangeAsync(cks);
        //    return d>0;
        //}

    }
}
