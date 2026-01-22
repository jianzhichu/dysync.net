using dy.net.model.dto;
using dy.net.model.entity;
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
    public class CateController : ControllerBase
    {
        private readonly DouyinCollectCateService _douyinCollectCateService;
        private readonly DouyinQuartzJobService _douyinQuartzJobService;

        public CateController(DouyinCollectCateService douyinCollectCateService, DouyinQuartzJobService douyinQuartzJobService)
        {
            this._douyinCollectCateService = douyinCollectCateService;
            _douyinQuartzJobService = douyinQuartzJobService;
        }



        /// <summary>
        /// 分页查询
        /// </summary>
        /// <returns>分页结果</returns>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedAsync(
           DouyinCollectCateRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.cookieId))
            {
                return ApiResult.Success(new { });
            }
            var (list, totalCount) = await _douyinCollectCateService.GetPagedAsync(dto);

            return ApiResult.Success(new
            {
                data = list,
                total = totalCount,
                pageIndex = dto.PageIndex,
                pageSize = dto.PageSize
            });
        }

        /// <summary>
        /// 修改关注同步状态
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("BatchSave")]
        public async Task<IActionResult> BatchSave(List<DouyinCollectCateSwitchDto> dto)
        {

            if(dto.Any(x=> !DouyinFileNameHelper.IsValidWithoutSpecialChars(x.Folder)))
            {
                return ApiResult.Fail("有部分文件名不符合要求，请检查");
            }

            var result=  await _douyinCollectCateService.BatchSwitchSync(dto);
            return ApiResult.SuccOrFail(result, result);
        }

    }
}
