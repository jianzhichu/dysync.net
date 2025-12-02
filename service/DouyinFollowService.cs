using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
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
        /// 
        /// </summary>
        /// <param name="followInfos"></param>
        /// <param name="myselfUserId"></param>
        /// <returns></returns>
        public async Task<bool> Sync(List<FollowingsItem> followInfos, string myselfUserId)
        {
            return await _followRepository.Sync(followInfos, myselfUserId);
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
