using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.sync.lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quartz.Util;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using static Dm.net.buffer.ByteArrayBuffer;

namespace dy.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConfigController : ControllerBase
    {
        private readonly DouyinCookieService dyCookieService;

        private readonly DouyinCommonService commonService;
        private readonly DouyinQuartzJobService quartzJobService;
        private readonly DouyinFollowService douyinFollowService;
        private readonly DouyinCookieService douyinCookieService;

        public ConfigController(DouyinCookieService dyCookieService, DouyinCommonService commonService, DouyinQuartzJobService quartzJobService, DouyinFollowService douyinFollowService, DouyinCookieService douyinCookieService)
        {
            this.dyCookieService = dyCookieService;
            this.commonService = commonService;
            this.quartzJobService = quartzJobService;
            this.douyinFollowService = douyinFollowService;
            this.douyinCookieService = douyinCookieService;
        }


        /// <summary>
        /// 导出配置
        /// </summary>
        /// <returns></returns>
        [HttpGet("exportConf")]
        public async Task<IActionResult> ExportConf()
        {

            // 1. 组装导出数据
            var dto = new AppConfigImportDto()
            {
                follows = await douyinFollowService.GetHandFollows(),
                conf = commonService.GetConfig(),
                cookies = await douyinCookieService.GetAllAsync()
            };

            return ApiResult.Success(dto);

        }


        /// <summary>
        /// 导入配置
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("importConf")]
        public async Task<IActionResult> ImportConf(AppConfigImportDto dto)
        {
            if (dto == null)
                return ApiResult.Fail("json数据为空");

            var follows = dto.follows;
            if (follows != null && follows.Count > 0)
            {
                var add = await douyinFollowService.AddHandFollows(follows);
                if (add)
                    Serilog.Log.Debug("关注列表导入成功");
            }

            var conf = dto.conf;
            if (conf != null)
            {
                var update = await commonService.UpdateConfig(conf);
                if (update)
                    Serilog.Log.Debug("系统配置导入成功");
            }
            var cookies = dto.cookies;

            if (cookies != null && cookies.Count > 0)
            {
                var importCookies = await douyinCookieService.ImportCookies(cookies);
                if (importCookies)
                {
                    Serilog.Log.Debug("抖音Cookie配置导入成功");
                }
            }

            return ApiResult.Success();
        }
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <returns>分页结果</returns>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedAsync(
           PageRequestDto dto)
        {
            var (list, totalCount) = await dyCookieService.GetPagedAsync(dto.PageIndex, dto.PageSize);
            return ApiResult.Success(new
            {
                data = list,
                total = totalCount,
                pageIndex = dto.PageIndex,
                pageSize = dto.PageSize
            });
        }


        /// <summary>
        /// 查询所有用户Cookie
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetAllList()
        {
            var follows = await douyinFollowService.GetGroupByCookieAsync();
            return ApiResult.Success(follows);
        }
        /// <summary>
        /// 新增用户Cookie
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddAsync([FromBody] DouyinCookie dyUserCookies)
        {

            var result = await dyCookieService.Add(dyUserCookies);
            if (result)
            {
                ReStartJob();
                return ApiResult.Success();
            }
            return ApiResult.Fail("添加失败");
        }

        /// <summary>
        /// 更新用户Cookie
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] DouyinCookie dyUserCookies)
        {

            if (dyUserCookies.Id == "0")
            {
                dyUserCookies.Id = IdGener.GetLong().ToString();
                var result = await dyCookieService.Add(dyUserCookies);
                if (result)
                {
                    ReStartJob();
                    return ApiResult.Success();
                }
                return ApiResult.Fail("添加失败");
            }
            else
            {
                var result = await dyCookieService.UpdateAsync(dyUserCookies);
                if (result)
                {
                    ReStartJob();
                    return ApiResult.Success();
                }
                return ApiResult.Fail("更新失败");
            }

        }

        /// <summary>
        /// 批量删除用户Cookie
        /// </summary>
        [HttpGet("delete")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var count = await dyCookieService.DeleteByIdsAsync(new List<string> { id });
            if (count > 0)
            {
                ReStartJob();
            }
            return ApiResult.Success(count);
        }

        [HttpGet("GetConfig")]
        public IActionResult GetConfig()
        {
            var data = commonService.GetConfig();
            return ApiResult.Success(data);
        }

        [HttpPost("UpdateConfig")]
        public async Task<IActionResult> UpdateConfig(AppConfig config)
        {
            var data = await commonService.UpdateConfig(config);
            if (data)
            {
                ReStartJob();
            }
            return ApiResult.Success(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("ExecuteJobNow")]
        //[Authorize]
        public async Task<IActionResult> ExecuteJobNow()
        {
            var config = commonService.GetConfig();
            if (config != null)
                await quartzJobService.InitOrReStartAllJobs(config.Cron.ToString());
            return ApiResult.Success();
        }


        private void ReStartJob()
        {
            var config = commonService.GetConfig();
            if (config != null)
                quartzJobService.InitOrReStartAllJobs(config.Cron.ToString());
            //避免前端等待
        }
        /// <summary>
        /// 镜像标签
        /// </summary>
        /// <returns></returns>
        [HttpGet("mytag")]
        public async Task<IActionResult> GetMyTag()
        {
            return ApiResult.Success(Appsettings.Get("tagName"));
        }

        [HttpGet("checktag")]
        public async Task<IActionResult> CheckTag()
        {
            var data = await DouyinHttpHelper.GetTenImage(Appsettings.Get("tagName"));
            if (data.IsSuccessStatusCode)
            {
                var content = await data.Content.ReadAsStringAsync();
                return Ok(content);
            }
            else
            {
                return ApiResult.Fail("请求失败");
            }
        }
    }
}
