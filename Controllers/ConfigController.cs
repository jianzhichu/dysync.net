using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
using dy.net.service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static Dm.net.buffer.ByteArrayBuffer;

namespace dy.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly DyCookieService dyCookieService;

        private readonly CommonService commonService;
        private readonly QuartzJobService quartzJobService;

        public ConfigController(DyCookieService dyCookieService, CommonService commonService,QuartzJobService quartzJobService)
        {
            this.dyCookieService = dyCookieService;
            this.commonService = commonService;
            this.quartzJobService = quartzJobService;
        }




        /// <summary>
        /// 分页查询
        /// </summary>
        /// <returns>分页结果（视频列表和总数）</returns>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedAsync(
           PageRequestDto dto)
        {
            var (list, totalCount) = await dyCookieService.GetPagedAsync(dto.PageIndex, dto.PageSize);
            return Ok(new
            {
                code = 0,
                data = new
                {
                    data = list,
                    total=totalCount,
                    pageIndex= dto.PageIndex,
                    pageSize= dto.PageSize
                }
            }
           );
        }
        /// <summary>
        /// 新增用户Cookie
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddAsync([FromBody] DyUserCookies dyUserCookies)
        {
           if(dyUserCookies.UpSecUserIdsJson!=null)
            {
                dyUserCookies.UpSecUserIds = JsonConvert.SerializeObject( dyUserCookies.UpSecUserIdsJson);
            }
            var result = await dyCookieService.Add(dyUserCookies);
            if (result)
            {
                await ReStartJob();
                return Ok(new { code = 0 });
            }
            return BadRequest(new { code=-1, message = "添加失败" });
        }

        /// <summary>
        /// 更新用户Cookie
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] DyUserCookies dyUserCookies)
        {

            if (dyUserCookies.UpSecUserIdsJson != null )
            {
                dyUserCookies.UpSecUserIds = JsonConvert.SerializeObject(dyUserCookies.UpSecUserIdsJson);
            }

            if (dyUserCookies.Id == "0")
            {
                dyUserCookies.Id=IdGener.GetLong().ToString();
                var result = await dyCookieService.Add(dyUserCookies);
                if (result)
                {
                    await ReStartJob();
                    return Ok(new { code = 0 });
                }
                return BadRequest(new { code = -1, message = "添加失败" });
            }
            else {
                var result = await dyCookieService.UpdateAsync(dyUserCookies);
                if (result)
                {
                    await ReStartJob();
                    return Ok(new { code = 0 });
                }
                return BadRequest(new { code = -1, message = "更新失败" });
            }
           
        }

        /// <summary>
        /// 批量删除用户Cookie
        /// </summary>
        [HttpGet("delete")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var count = await dyCookieService.DeleteByIdsAsync(new List<string> { id});
            if (count > 0) {
                await ReStartJob();
            }
            return Ok(new { code = 0, deletedCount = count });
        }

        [HttpGet("GetConfig")]
        public IActionResult GetConfig()
        {
            var data =  commonService.GetConfig();
            return Ok(new { code = 0, data = data });
        }

        [HttpPost("UpdateConfig")]
        public async Task<IActionResult> UpdateConfig(AppConfig config)
        {
            var data = await commonService.UpdateConfig(config);
            if (data) {
                await ReStartJob();
            }
            return Ok(new { code = 0, data = data });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("ExecuteJobNow")]
        [Authorize]
        public async Task<IActionResult> ExecuteJobNow()
        {
            var config =  commonService.GetConfig();
            await quartzJobService.StartJob(config.Cron);
            return Ok(new { code =  0 , error =  ""  });
        }


        private async Task ReStartJob() {
            var config= commonService.GetConfig();

            //var cookies = await dyCookieService.GetAllCookies();
            //重置同步状态
            //foreach (var cookie in cookies)
            //{
            //    cookie.CollHasSyncd = 0;
            //    cookie.FavHasSyncd = 0;
            //    cookie.UperSyncd = 0;
            //    await dyCookieService.UpdateAsync(cookie);
            //}
          
            if (config!=null)
             quartzJobService.StartJob(config.Cron);
            //避免前端等待
        }
    }
}
