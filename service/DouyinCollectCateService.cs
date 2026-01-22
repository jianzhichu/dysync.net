using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.repository;
using SqlSugar;
using System.Linq.Expressions;

namespace dy.net.service
{
    public class DouyinCollectCateService
    {

        private readonly DouyinCollectCateRepository _douyinCollectCateRepository;

        public DouyinCollectCateService(DouyinCollectCateRepository douyinCollectCateRepository)
        {
            _douyinCollectCateRepository = douyinCollectCateRepository;
        }



        public async Task<bool> AddAsync(DouyinCollectCate cate)
        {
            return await _douyinCollectCateRepository.InsertAsync(cate);
        }


       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<(List<DouyinCollectCate> list, int totalCount)> GetPagedAsync(DouyinCollectCateRequestDto dto)
        {
            return await _douyinCollectCateRepository.GetPagedAsync(dto);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cates"></param>
        /// <param name="ckId"></param>
        /// <param name="cateType"></param>
        /// <returns></returns>
        public async Task<(int add, int update,int delete, bool succ)> Sync(List<DouyinCollectCate> cates, string ckId,VideoTypeEnum cateType)
        {
            return await _douyinCollectCateRepository.Sync(cates, ckId, cateType);
        }


        /// <summary>
        /// 打开或关闭同步
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> BatchSwitchSync(List<DouyinCollectCateSwitchDto> dto)
        {
            return await _douyinCollectCateRepository.SwitchBatchAsync(dto);
        }




        /// <summary>
        /// 获取需要同步的合集、短剧、收藏夹
        /// </summary>
        /// <returns></returns>
        public async Task<List<DouyinCollectCate>> GetSyncCates(string cookieId, VideoTypeEnum cateType)
        {
            return await _douyinCollectCateRepository.GetListAsync(x => x.CookieId == cookieId && x.CateType == cateType && x.Sync && x.Total > 0);
        }
        /// <summary>
        /// 完结
        /// </summary>
        /// <param name="cate"></param>
        /// <returns></returns>

        public async Task<bool> UpdateCate2EndStatus(DouyinCollectCate cate)
        {
            return await _douyinCollectCateRepository.UpdateCateEndStatus(cate);
        }

    }
}
