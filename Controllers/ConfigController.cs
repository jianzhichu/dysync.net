using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
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

        //// 定义允许上传的音频扩展名（小写）
        //private readonly string[] _allowedAudioExtensions = { ".mp3", ".wav" };

        //// 定义允许的音频 MIME 类型（增强验证）
        //private readonly string[] _allowedAudioMimeTypes = {
        //    "audio/mpeg", "audio/wav"
        //};

        // 最大文件大小：20MB（可根据需求调整）
        //private const long _maxFileSize = 20 * 1024 * 1024;


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
        /// 是否已经初始化了
        /// </summary>
        /// <returns></returns>
        [HttpGet("isInit")]
        [AllowAnonymous]
        public async Task<IActionResult> IsInit()
        {
            var init = await dyCookieService.IsInit();
            return ApiResult.Success(init);
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
        /// 非docker初始化
        /// </summary>
        [HttpPost("deskinit")]
        [AllowAnonymous]
        public async Task<IActionResult> DeskInitAsync([FromBody] DouyinCookie dyUserCookies)
        {
            dyUserCookies.Id = IdGener.GetLong().ToString();

            if (string.IsNullOrWhiteSpace(dyUserCookies.SavePath))
            {
                return ApiResult.Fail("收藏存储路径不能为空");
            }
            if (!DouyinFileUtils.HasDirectoryReadWritePermission(dyUserCookies.SavePath))
            {
                return ApiResult.Fail($"请在飞牛应用设置里面将{dyUserCookies.SavePath}添加读写权限");
            }

            if (!string.IsNullOrWhiteSpace(dyUserCookies.FavSavePath) && !DouyinFileUtils.HasDirectoryReadWritePermission(dyUserCookies.FavSavePath))
            {
                return ApiResult.Fail($"请在飞牛应用设置里面将{dyUserCookies.FavSavePath}添加读写权限");
            }

            if (!string.IsNullOrWhiteSpace(dyUserCookies.UpSavePath) && !DouyinFileUtils.HasDirectoryReadWritePermission(dyUserCookies.UpSavePath))
            {
                return ApiResult.Fail($"请在飞牛应用设置里面将{dyUserCookies.UpSavePath}添加读写权限");
            }

            if (!string.IsNullOrWhiteSpace(dyUserCookies.ImgSavePath) && !DouyinFileUtils.HasDirectoryReadWritePermission(dyUserCookies.ImgSavePath))
            {
                return ApiResult.Fail($"请在飞牛应用设置里面将{dyUserCookies.ImgSavePath}添加读写权限");
            }

            var result = await dyCookieService.Add(dyUserCookies);
            if (result)
            {

                return ApiResult.Success();
            }
            return ApiResult.Fail("添加失败");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("appport")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAppPort()
        {
            //return ApiResult.Success(10105);
            return ApiResult.Success(string.IsNullOrWhiteSpace(Appsettings.Get("appPort")) ? 10101 : Convert.ToInt32(Appsettings.Get("appPort")));
        }




        /// <summary>
        /// 快速开启或停止
        /// </summary>
        [HttpPost("switch")]
        public async Task<IActionResult> SwitchAsync([FromBody] DouyinCookieStopDto dto)
        {
            var result = await dyCookieService.Switch(dto);
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
                if (config.OnlySyncNew)
                {
                    var d = await douyinCookieService.SetOnlySyncNew();
                    if (d)
                        Serilog.Log.Debug("仅同步新视频配置已生效,后续所有类型的视频同步将只会读取最近一页约20条数据");
                }

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

        [AllowAnonymous]
        [HttpGet("checktag")]
        public async Task<IActionResult> CheckTag()
        {

            var deploy = Appsettings.Get("deploy");
            if (string.IsNullOrWhiteSpace(deploy))
            {
                return await GetDockerTagVersions();
            }
            else
            {
                if (deploy == "fn")//飞牛
                {
                    return ApiResult.Success(new List<string> { "beta_" + Appsettings.Get("fnVersion") });
                }
                else
                {
                    return await GetDockerTagVersions();
                }
            }


        }

        private static async Task<IActionResult> GetDockerTagVersions()
        {
            var data = await DouyinHttpHelper.GetTenImage(Appsettings.Get("tagName"));
            if (data.IsSuccessStatusCode)
            {
                var content = await data.Content.ReadAsStringAsync();

                var tagData = JsonConvert.DeserializeObject<DouyinApiResponse<List<string>>>(content);
                if (tagData != null && tagData.Data != null && tagData.Data.Count > 0)
                    return ApiResult.Success(tagData.Data);
                return ApiResult.Fail();
            }
            else
            {
                return ApiResult.Fail("请求失败");
            }
        }

        /// <summary>
        /// 查询mp3目录下有没有音频文件
        /// </summary>
        /// <returns></returns>
        [HttpGet("mp3List")]
        public async Task<IActionResult> GetExistMps()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "mp3");
            if (Directory.Exists(path))
            {
                var audioFiles = Directory.GetFiles(path);

                var fileNames = audioFiles.Select(f => new {filename= Path.GetFileName(f) }).Where(x=>x.filename!= "silent_10.mp3").ToList();
                return ApiResult.Success(fileNames);
            }
            else
            {
                return ApiResult.Fail("没有找到默认音频文件");
            }
        }

        /// <summary>
        /// 播放音频流
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("getmp3")]
        public async Task<IActionResult> GetMp3([FromQuery] string name)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "mp3", name);
            if (System.IO.File.Exists(path))
            {
                //返回mp3文件流
                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                return new FileStreamResult(fileStream, "audio/mpeg")
                {
                    FileDownloadName = name
                };
            }
            else
            {
                return ApiResult.Fail("文件不存在");
            }
        }
    }
}
