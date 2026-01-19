using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.model.response;
using dy.net.repository;
using SqlSugar;
using System.Linq.Expressions;

namespace dy.net.service
{
    public class DouyinFollowService
    {

        private readonly DouyinFollowRepository _followRepository;

        public DouyinFollowService(DouyinFollowRepository followRepository)
        {
            _followRepository = followRepository;
        }



        public async Task<List<DouyinFollowGroupDto>> GetGroupByCookieAsync()
        {
            return await _followRepository.GetDouyinFollowGroup();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="followed"></param>
        /// <returns></returns>
        public async Task<bool> AddAsync(DouyinFollowed followed)
        {
          var foll= await _followRepository.GetFirstAsync(x=>x.SecUid==followed.SecUid && x.mySelfId== followed.mySelfId);
            if(foll!=null)
            {
                return false;
            }
            followed.Id = IdGener.GetLong().ToString();
            followed.LastSyncTime = DateTime.UtcNow;
            followed.IsNoFollowed = true;
            return await _followRepository.InsertAsync(followed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<(List<DouyinFollowed> list, int totalCount)> GetPagedAsync(FollowRequestDto dto)
        {
            return await _followRepository.GetPagedAsync(dto);
        }

        /// <summary>
        /// 获取所有手动添加的非关注者
        /// </summary>
        /// <returns></returns>
        public async Task<List<DouyinFollowed>> GetHandFollows()
        {
            return await _followRepository.GetListAsync(x=>x.IsNoFollowed);
        }
        /// <summary>
        /// 导入非关注
        /// </summary>
        /// <param name="douyinFolloweds"></param>
        /// <returns></returns>
        public async Task<bool> AddHandFollows(List<DouyinFollowed> douyinFolloweds)
        {
            if (douyinFolloweds == null || !douyinFolloweds.Any()) return false;

            var ids = douyinFolloweds.Select(x => x.Id).Distinct().ToList();
            var existingFolloweds = await _followRepository.GetListAsync(x => ids.Contains(x.Id));

            var toAddList = douyinFolloweds
                .Where(x => !existingFolloweds.Any(e => e.Id == x.Id))
                .ToList();

            // 批量更新：先整理需要更新的实体
            var toUpdateEntities = new List<DouyinFollowed>();
            foreach (var updateItem in douyinFolloweds)
            {
                var existingItem = existingFolloweds.FirstOrDefault(e => e.Id == updateItem.Id);
                if (existingItem != null)
                {
                    toUpdateEntities.Add(updateItem);
                }
            }

            int operateCount = 0;
            // 批量新增
            if (toAddList.Any())
            {
                operateCount += await _followRepository.InsertRangeAsync(toAddList);
            }
            // 批量更新
            if (toUpdateEntities.Any())
            {
                await _followRepository.UpdateRangeAsync(toUpdateEntities);
                operateCount += toUpdateEntities.Count;
            }

            return operateCount > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="followInfos"></param>
        /// <param name="ck"></param>
        /// <returns></returns>
        public async Task<(int add, int update, bool succ)> Sync(List<FollowingsItem> followInfos, DouyinCookie ck)
        {
            return await _followRepository.Sync(followInfos, ck);
        }

        public async Task<DouyinFollowed> GetByUperId(string uperId,string myUid)
        { 
            return await _followRepository.GetBySecUId(uperId, myUid);
        }

        /// <summary>
        /// 打开或关闭同步
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> OpenOrCloseSync(FollowUpdateDto dto)
        {
            var followed = await _followRepository.GetByIdAsync(dto.Id);
            if (followed != null)
            {
                followed.OpenSync = dto.OpenSync;
                followed.FullSync = dto.FullSync;
                followed.SavePath = dto.SavePath;
                return await _followRepository.Update(followed);
            }
            return false;
        }



        public async Task<bool> DeleteFollow(FollowUpdateDto dto)
        {
            return await _followRepository.DeleteByIdAsync(dto.Id);
        }
       

        /// <summary>
        /// 打开或关闭同步
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> OpenOrCloseFullSync(FollowUpdateDto dto)
        {
            return await OpenOrCloseSync(dto);
        }

        /// <summary>
        /// 获取需要同步的关注列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<DouyinFollowed>> GetSyncFollows(string userUserId)
        {
            return await _followRepository.GetSyncFollows(userUserId);
        }

    }
}
