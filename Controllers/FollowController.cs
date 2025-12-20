using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace dy.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly DouyinFollowService _douyinFollowService;
        private readonly DouyinQuartzJobService _douyinQuartzJobService;

        public FollowController(DouyinFollowService douyinFollowService, DouyinQuartzJobService douyinQuartzJobService)
        {
            this._douyinFollowService = douyinFollowService;
            _douyinQuartzJobService = douyinQuartzJobService;
        }



        /// <summary>
        /// 分页查询
        /// </summary>
        /// <returns>分页结果</returns>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedAsync(
           FollowRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MySelfId))
            {
                return ApiResult.Success(new { });
            }
            var (list, totalCount) = await _douyinFollowService.GetPagedAsync(dto);

            return ApiResult.Success(new
            {
                data = list,
                total = totalCount,
                pageIndex = dto.PageIndex,
                pageSize = dto.PageSize
            });
        }
        /// <summary>
        /// 重新同步-单次
        /// </summary>
        /// <returns></returns>
        [HttpGet("sync")]
        public async Task<IActionResult> SyncFollowList()
        {
            //后台异步
            _douyinQuartzJobService.StartFollowJobOnceAsync();
            await Task.Delay(1000);
            return ApiResult.Success();
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddFollow(DouyinFollowed followed)
        {
            var res = await _douyinFollowService.AddAsync(followed);
            return ApiResult.SuccOrFail(res,"", res ? "" : "添加失败,或者已存在相同secuid和uid");
        }

        /// <summary>
        /// 修改关注同步状态
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("openOrCloseSync")]
        public async Task<IActionResult> OpenOrCloseSync(FollowUpdateDto dto)
        {

            if (dto.OpenSync)
            {
                if (!string.IsNullOrWhiteSpace(dto.SavePath))
                {
                    if (!DouyinFileNameHelper.IsValidWithoutSpecialChars(dto.SavePath))
                    {
                        return ApiResult.Fail("请输入有效文件夹名称（字母数字中文简体）");
                    }

                    if (dto.SavePath.Length > 15)
                    {
                        return ApiResult.Fail("请输入有效文件夹名称（最长15）");
                    }
                }
            }
            var result=  await _douyinFollowService.OpenOrCloseSync(dto);
            return ApiResult.SuccOrFail(result, result);
        }

        /// <summary>
        /// 修改关注全量同步状态
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("openOrCloseFullSync")]
        public async Task<IActionResult> OpenOrCloseFullSync(FollowUpdateDto dto)
        {
            return await OpenOrCloseSync(dto);
        }


        [HttpPost("delete")]
        public async Task<IActionResult> DeleteFollow(FollowUpdateDto dto)
        {
            var result= await _douyinFollowService.DeleteFollow(dto);
            return ApiResult.SuccOrFail(result, result);
        }
    }
}
