using ClockSnowFlake;
using dy.net.extension;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.service;
using dy.net.utils;
using jzc.http.lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;

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
        private readonly DouyinHttpClientService httpClientService;
        private readonly DatabaseMigrationService databaseMigrationService;
        private readonly DatabaseMigrationJobService databaseMigrationJobService;
        private readonly ApplicationRestartService applicationRestartService;
        private readonly DouyinQrLoginService qrLoginService;



        public ConfigController(
            DouyinCookieService dyCookieService,
            DouyinCommonService commonService,
            DouyinQuartzJobService quartzJobService,
            DouyinFollowService douyinFollowService,
            DouyinCookieService douyinCookieService,
            DouyinHttpClientService httpClientService,
            DatabaseMigrationService databaseMigrationService,
            DatabaseMigrationJobService databaseMigrationJobService,
            ApplicationRestartService applicationRestartService,
            DouyinQrLoginService qrLoginService)
        {
            this.qrLoginService = qrLoginService;
            this.dyCookieService = dyCookieService;
            this.commonService = commonService;
            this.quartzJobService = quartzJobService;
            this.douyinFollowService = douyinFollowService;
            this.douyinCookieService = douyinCookieService;
            this.httpClientService = httpClientService;
            this.databaseMigrationService = databaseMigrationService;
            this.databaseMigrationJobService = databaseMigrationJobService;
            this.applicationRestartService = applicationRestartService;
        }

        /// <summary>当前持久化数据库状态。连接字符串不会返回前端。</summary>
        [HttpGet("database/status")]
        public IActionResult GetDatabaseStatus()
        {
            return ApiResult.Success(databaseMigrationService.GetStatus());
        }

        /// <summary>首次登录时确认继续使用 SQLite，并持久化用户选择。</summary>
        [HttpPost("database/select-sqlite")]
        public IActionResult SelectSqliteDatabase()
        {
            try
            {
                databaseMigrationService.ConfirmSqliteSelection();
                return ApiResult.Success();
            }
            catch (Exception ex)
            {
                return ApiResult.Fail(ex.GetBaseException().Message);
            }
        }

        /// <summary>将当前数据库的业务数据迁移到 SQLite/MySQL/PostgreSQL。</summary>
        [HttpPost("database/migrate")]
        public IActionResult MigrateDatabase([FromBody] DatabaseMigrationRequest request)
        {
            try
            {
                var status = databaseMigrationJobService.Start(request);
                return ApiResult.Success(status, "迁移任务已提交，请稍后刷新查看进度");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "数据库迁移失败");
                return ApiResult.Fail(ex.GetBaseException().Message);
            }
        }

        /// <summary>查询后台数据库迁移进度；同一用户每 5 秒最多查询一次。</summary>
        [HttpGet("database/migration/status")]
        public IActionResult GetDatabaseMigrationStatus()
        {
            var clientKey = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.Identity?.Name
                ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous";

            if (!databaseMigrationJobService.TryGetStatus(
                clientKey, out var status, out var retryAfterSeconds))
            {
                return ApiResult.Fail(
                    $"刷新过于频繁，请 {retryAfterSeconds} 秒后再试",
                    data: new { retryAfterSeconds });
            }

            return ApiResult.Success(status);
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


        [HttpPost("importConf")]
        public async Task<IActionResult> ImportConf(AppConfigImportDto dto)
        {
            if (dto == null)
                return ApiResult.Fail("json数据为空");

            await HandleFollowsImport(dto.follows);
            await HandleConfigImport(dto.conf);
            await HandleCookiesImport(dto.cookies);

            return ApiResult.Success();
        }

        private async Task HandleFollowsImport(List<DouyinFollowed> follows)
        {
            if (follows?.Count > 0)
            {
                var added = await douyinFollowService.AddHandFollows(follows);
                if (added)
                    Serilog.Log.Debug("关注列表导入成功");
            }
        }

        private async Task HandleConfigImport(AppConfig conf)
        {
            if (conf == null) return;

            if (conf.BatchCount > 30)
            {
                Serilog.Log.Debug("对不起，为了项目能长久稳定运行，还是最大不要超过30吧。。。");
                conf.BatchCount = 30;
            }

            var updated = await commonService.UpdateConfig(conf);
            if (updated)
                Serilog.Log.Debug("系统配置导入成功");
        }

        private async Task HandleCookiesImport(List<DouyinCookie> cookies)
        {
            if (cookies?.Count > 0)
            {
                var imported = await douyinCookieService.ImportCookies(cookies);
                if (imported)
                    Serilog.Log.Debug("抖音Cookie配置导入成功");
            }
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
        /// 配合工具自动设置cookie
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("FastResetCookie")]
        [AllowAnonymous]
        public async Task<IActionResult> FastResetCookie([FromBody] DouyinCookieResetDto dto)
        {
            var cookieValid = await httpClientService.CheckCookie(new DouyinCookie { Cookies = dto.cookie });
            if (!cookieValid)
                return ApiResult.Fail("Cookie无效或已过期，请按照文档提示重新获取有效Cookie，不要使用插件获取cookie");
            var result = await dyCookieService.FastResetCookie(dto.id, dto.cookie);
         
            return result ? ApiResult.Success() : ApiResult.Fail("cookie设置失败");
        }

        /// <summary>
        /// 配合工具自动设置cookie,获取当前设置的所有Cookie
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("Cookies")]
        public async Task<IActionResult> GetAllCookies()
        {
            var cookies = await dyCookieService.GetAllAsync();
            if (cookies != null)
                return ApiResult.Success(cookies.Select(x => new { id= x.Id,name= x.UserName, status=x.StatusMsg }));
            return ApiResult.Success();
        }


        /// <summary>
        /// 开始扫码登录：后台打开抖音登录页，返回会话 id
        /// </summary>
        [HttpPost("qr/start")]
        public IActionResult StartQrLogin()
        {
            return ApiResult.Success(qrLoginService.Start());
        }

        /// <summary>
        /// 轮询扫码状态。v 为前端已拿到的二维码版本号，版本没变时不重复返回图片
        /// </summary>
        [HttpGet("qr/status")]
        public IActionResult GetQrLoginStatus([FromQuery] string id, [FromQuery] int v = 0)
        {
            var status = qrLoginService.GetStatus(id, v);
            return status == null ? ApiResult.Fail("扫码会话不存在或已结束，请重新获取二维码") : ApiResult.Success(status);
        }

        /// <summary>
        /// 扫码浏览器的当前画面，二维码图片取不到或需要查看页面提示时使用
        /// </summary>
        [HttpGet("qr/screenshot")]
        public async Task<IActionResult> GetQrLoginScreenshot([FromQuery] string id)
        {
            var image = await qrLoginService.CaptureScreenshotAsync(id);
            // 包成对象返回：ApiResult.Success(string) 会把字符串当成 message
            return image == null ? ApiResult.Fail("暂时无法获取浏览器画面") : ApiResult.Success(new { image });
        }

        /// <summary>
        /// 把前端在浏览器画面上的点击、拖动、输入转发给扫码浏览器，用于完成抖音的身份验证
        /// </summary>
        [HttpPost("qr/action")]
        public async Task<IActionResult> PerformQrLoginAction([FromQuery] string id, [FromBody] DouyinQrLoginActionDto action)
        {
            var done = await qrLoginService.PerformActionAsync(id, action);
            return done ? ApiResult.Success() : ApiResult.Fail("操作未能发送到浏览器");
        }

        /// <summary>
        /// 取消扫码并释放浏览器
        /// </summary>
        [HttpPost("qr/cancel")]
        public IActionResult CancelQrLogin([FromQuery] string id)
        {
            qrLoginService.Cancel(id);
            return ApiResult.Success();
        }

        /// <summary>
        /// 非docker初始化
        /// </summary>
        [HttpPost("deskinit")]
        [AllowAnonymous]
        public async Task<IActionResult> DeskInitAsync(
            [FromBody] DeskInitRequest dyUserCookies,
            CancellationToken cancellationToken)
        {
            // 1. 基础赋值
            dyUserCookies.Id = IdGener.GetLong().ToString();

            // 2. 路径权限校验
            var (Success, Message) = ValidatePaths(dyUserCookies);
            if (!Success)
                return ApiResult.Fail(Message);

            RemoveCookieLineString(dyUserCookies);

            // 3. Cookie 有效性校验
            var cookieValid = await httpClientService.CheckCookie(dyUserCookies);
            if (!cookieValid)
                return ApiResult.Fail("Cookie无效或已过期，请按照文档提示重新获取有效Cookie，不要使用插件获取cookie");

            dyUserCookies.StatusCode = 0;
            dyUserCookies.StatusMsg = "正常";
            // 4. 保存到数据库
            var saved = await dyCookieService.Add(dyUserCookies);
            if (!saved)
            {
                return ApiResult.Fail("添加失败");
            }

            if (!string.Equals(
                DatabaseKinds.Normalize(dyUserCookies.DatabaseType),
                DatabaseKinds.Sqlite,
                StringComparison.Ordinal))
            {
                try
                {
                    var migration = await databaseMigrationService.MigrateAsync(
                        new DatabaseMigrationRequest
                        {
                            DbType = dyUserCookies.DatabaseType,
                            ConnectionString = dyUserCookies.DatabaseConnectionString,
                            Host = dyUserCookies.DatabaseHost,
                            Port = dyUserCookies.DatabasePort,
                            UserName = dyUserCookies.DatabaseUserName,
                            Password = dyUserCookies.DatabasePassword,
                            DatabaseName = dyUserCookies.DatabaseName
                        }, cancellationToken);
                    applicationRestartService.RestartAfterResponse();
                    return ApiResult.Success(migration, "初始化及数据库迁移成功，后台服务正在热重启");
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "初始化阶段数据库迁移失败");
                    return ApiResult.Fail($"Cookie 配置已保存，但数据库迁移失败：{ex.GetBaseException().Message}");
                }
            }

            return ApiResult.Success();
        }

        private static (bool Success, string Message) ValidatePaths(DouyinCookie cookie)
        {
            var pathsToCheck = new Dictionary<string, string>
    {
        { "收藏存储路径", cookie.SavePath },
        { "喜欢视频存储路径", cookie.FavSavePath },
        { "上传视频存储路径", cookie.UpSavePath },
        // { "图片存储路径", cookie.ImgSavePath } // 可随时启用
    };

            foreach (var (label, path) in pathsToCheck)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (!DouyinFileUtils.HasDirectoryReadWritePermission(path))
                    {
                        return (false, $"请在飞牛应用设置里面将 {path} 添加读写权限（{label}）");
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(cookie.SavePath))
            {
                return (false, "收藏存储路径不能为空");
            }

            return (true, string.Empty);
        }


        /// <summary>
        /// 快速开启或停止
        /// </summary>
        [HttpPost("switch")]
        public async Task<IActionResult> SwitchAsync([FromBody] DouyinCookieSwitchDto dto)
        {
            var result = await dyCookieService.Switch(dto);
            if (result)
            {
                await ReStartJobAsync();
                return ApiResult.Success();
            }
            return ApiResult.Fail("添加失败");
        }
        /// <summary>
        /// 更新用户Cookie
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> AddOrUpdateAsync([FromBody] DouyinCookie dyUserCookies)
        {
            RemoveCookieLineString(dyUserCookies);

            var checkCk = await httpClientService.CheckCookie(dyUserCookies);
            if (!checkCk)
            {
                return ApiResult.Fail("Cookie无效或已过期，请按照文档提示重新获取有效Cookie，不要使用插件获取cookie");
            }
            dyUserCookies.StatusCode = 0;
            dyUserCookies.StatusMsg = "正常";
            if (dyUserCookies.Id == "0")
            {
                dyUserCookies.Id = IdGener.GetLong().ToString();
                var result = await dyCookieService.Add(dyUserCookies);
                if (result)
                {
                    await ReStartJobAsync();
                    return ApiResult.Success();
                }
                return ApiResult.Fail("添加失败");
            }
            else
            {
                var result = await dyCookieService.UpdateCookieAsync(dyUserCookies);
                if (result)
                {
                    await ReStartJobAsync();
                    return ApiResult.Success();
                }
                return ApiResult.Fail("更新失败");
            }

        }

        private static void RemoveCookieLineString(DouyinCookie dyUserCookies)
        {
            if (dyUserCookies != null && !string.IsNullOrWhiteSpace(dyUserCookies.Cookies))
            {
                var s = dyUserCookies.Cookies.Replace("\\r\\n", "\r\n");
                var ss = s.Trim(new char[] { '\r', '\n' });
                dyUserCookies.Cookies = ss;
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
                await ReStartJobAsync();
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
            var update = await commonService.UpdateConfig(config);
            if (update)
            {
                if (config.OnlySyncNew)
                {
                    Serilog.Log.Debug("仅同步新视频配置已生效,后续所有类型的视频同步将只会读取最近一页约20条数据");
                }

                await ReStartJobAsync();
                return ApiResult.Success(true);
            }
            return ApiResult.Fail("配置未更新，请刷新页面后重试");
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


        private async Task ReStartJobAsync()
        {
            var config = commonService.GetConfig();
            if (config != null)
                await quartzJobService.InitOrReStartAllJobs(config.Cron.ToString());
        }
        /// <summary>
        /// 镜像标签
        /// </summary>
        /// <returns></returns>
        [HttpGet("mytag")]
        public async Task<IActionResult> GetMyTag()
        {
            var deploy = Appsettings.Get("deploy");
            var tag = Appsettings.Get("tagName");
            tag = deploy == "fn" ? "fn_" + Appsettings.Get("fnVersion") : tag;
            deploy = deploy == "fn" ? "fnos" : "docker";
            return ApiResult.Success(new { tag, deploy });
        }

        /// <summary>列出 Docker Compose 中挂载到容器、且挂载目录名以 dyapp_ 开头的扩展存储根目录。</summary>
        [HttpGet("storage/expansion-roots")]
        public IActionResult GetDockerStorageExpansionRoots()
        {
            var roots = new SortedSet<string>(StringComparer.Ordinal);
            if (!string.Equals(Appsettings.Get("deploy"), "fn", StringComparison.OrdinalIgnoreCase)
                && OperatingSystem.IsLinux()
                && System.IO.File.Exists("/proc/self/mountinfo"))
            {
                foreach (var line in System.IO.File.ReadLines("/proc/self/mountinfo"))
                {
                    var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length < 5) continue;

                    var mountPath = fields[4]
                        .Replace("\\040", " ")
                        .Replace("\\011", "\t")
                        .Replace("\\012", "\n")
                        .Replace("\\134", "\\");
                    var directoryName = Path.GetFileName(mountPath.TrimEnd(Path.DirectorySeparatorChar));
                    if (directoryName.StartsWith("dyapp_", StringComparison.OrdinalIgnoreCase)
                        && Directory.Exists(mountPath))
                    {
                        roots.Add(mountPath);
                    }
                }
            }

            return ApiResult.Success(roots.ToArray());
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
                    return ApiResult.Success(new List<string> { "fn_" + Appsettings.Get("fnVersion") });
                }
                else
                {
                    return await GetDockerTagVersions();
                }
            }


        }

        private static async Task<IActionResult> GetDockerTagVersions()
        {
            var data = await JianZhiChuHttpHelper.GetTenImage(Appsettings.Get("tagName"));
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

            if (!string.IsNullOrWhiteSpace(ServiceExtension.FnDataFolder))
            {
                path = ServiceExtension.FnDataFolder;
            }

            if (Directory.Exists(path))
            {
                var allowedExtensions = new HashSet<string> { ".mp3", ".wav" };

                var customMusics = Directory.GetFiles(path)
                 .Where(filePath =>
                     allowedExtensions.Contains(Path.GetExtension(filePath).ToLowerInvariant()) &&
                     Path.GetFileNameWithoutExtension(filePath) != "silent_10")
                 .ToList();

                var fileNames = customMusics.Select(f => new { filename = Path.GetFileName(f) }).Where(x => x.filename != "silent_10.mp3").ToList();
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
            if (!string.IsNullOrWhiteSpace(ServiceExtension.FnDataFolder))
            {
                path = Path.Combine(ServiceExtension.FnDataFolder, name);
            }

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
