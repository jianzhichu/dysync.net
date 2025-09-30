using dy.net.dto;
using dy.net.service;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;

namespace dy.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly DyCollectVideoService dyCollectVideoService;

        public VideoController(DyCollectVideoService dyCollectVideoService)
        {
            this.dyCollectVideoService = dyCollectVideoService;
        }
        /// <summary>
        /// 分页查询收藏视频
        /// </summary>
        /// <param name="dto"></param>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedAsync(VideoPageRequestDTO dto)
        {
            var (list, totalCount) = await dyCollectVideoService.GetPagedAsync(dto.PageIndex, dto.PageSize, dto.Tag, dto.Author,dto.ViedoType,dto.Dates);
            return Ok(new
            {
                code = 0,
                data = new
                {
                    data = list,
                    total=totalCount,
                    pageIndex=dto.PageIndex,
                    pageSize=dto.PageSize
                }
            });
        }

        /// <summary>
        /// 查询统计数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("statics")]
        public async Task<IActionResult> GetStaticsAsync()
        {
            var data = await dyCollectVideoService.GetStatics();
            return Ok(new
            {
                code = 0,
                data
            });
        }


    }
}
