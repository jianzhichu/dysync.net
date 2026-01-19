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

        private readonly DouyinCollectCateRepository douyinCollectCateRepository;

        public DouyinCollectCateService(DouyinCollectCateRepository douyinCollectCateRepository)
        {
            douyinCollectCateRepository = douyinCollectCateRepository;
        }




       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<(List<DouyinCollectCate> list, int totalCount)> GetPagedAsync(DouyinCollectCateRequestDto dto)
        {
            return await douyinCollectCateRepository.GetPagedAsync(dto);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cates"></param>
        /// <param name="ckId"></param>
        /// <returns></returns>
        public async Task<(int add, int update,int delete, bool succ)> Sync(List<DouyinCollectCate> cates, string ckId)
        {
            return await douyinCollectCateRepository.Sync(cates, ckId);
        }


        /// <summary>
        /// 打开或关闭同步
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> BatchSwitchSync(List<DouyinCollectCateSwitchDto> dto)
        {
            return await douyinCollectCateRepository.SwitchBatchAsync(dto);
        }




        /// <summary>
        /// 获取需要同步的合集、短剧、收藏夹
        /// </summary>
        /// <returns></returns>
        public async Task<List<DouyinCollectCate>> GetSyncCates(string cookieId,int cateType)
        {
            return await douyinCollectCateRepository.GetListAsync(x => x.CookieId == cookieId && x.CateType == cateType);
        }

    }
}
