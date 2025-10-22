using ClockSnowFlake;
using dy.net.dto;
using dy.net.extension;
using dy.net.model;
using dy.net.service;
using dy.net.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SqlSugar;
using System.Reflection;
using System.Text;

namespace dy.net
{
    public class Program
    {
        // 常量定义
        private const string DefaultListenUrl = "http://*:10101";
        private const string SpaRootPath = "app/dist";
        private const string SpaSourcePath = "app/";
        private const string SwaggerDocTitle = "dy.net WebApi Docs";

        public static void Main(string[] args)
        {
            // 初始化编码提供器
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // 构建Web应用
            var builder = WebApplication.CreateBuilder(args);
            var isDevelopment = builder.Environment.IsDevelopment();

            // 配置主机
            ConfigureHost(builder, isDevelopment);

            // 配置服务
            var services = builder.Services;
            ConfigureServices(services, builder.Configuration, isDevelopment);

            // 构建应用
            var app = builder.Build();

            // 配置中间件
            ConfigureMiddleware(app, isDevelopment);

            // 初始化应用服务
            InitApplicationServices(app);

            // 启动应用
            Serilog.Log.Debug("dy.net service started successfully");
            app.Run();
        }

        /// <summary>
        /// 配置主机设置
        /// </summary>
        private static void ConfigureHost(WebApplicationBuilder builder, bool isDevelopment)
        {
            // 设置监听地址
            builder.WebHost.UseUrls(DefaultListenUrl);

            // 配置配置文件
            builder.Host.ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddEnvironmentVariables();
            });

            // 配置日志
            builder.Host.ConfigureLogging(logging => logging.ClearProviders())
                       .UseSerilog();

            builder.ConfigureLogging();
        }

        /// <summary>
        /// 配置依赖注入服务
        /// </summary>
        private static void ConfigureServices(IServiceCollection services, IConfiguration config, bool isDevelopment)
        {
            // 雪花ID生成器
            services.AddSnowFlakeId(options => options.WorkId = new Random().Next(1, 127));

            // MVC控制器
            services.AddControllers();

            // HTTP客户端
            services.AddHttpClients();

            // 数据库
            services.AddSqlsugar(config);

            // 定时任务
            services.AddQuartzService(config);

            // 仓储和服务注册
            services.AddServicesFromNamespace("dy.net.repository")
                    .AddServicesFromNamespace("dy.net.service");

            // SPA静态文件支持
            services.AddSpaStaticFiles(options => options.RootPath = SpaRootPath);

            // 开发环境启用Swagger
            if (isDevelopment)
            {
                services.AddSwagger();
            }

            // 响应压缩
            services.AddResponseCompression();

            // JWT认证
            services.ConfigureJwtAuthentication();
        }

      

        /// <summary>
        /// 配置中间件
        /// </summary>
        private static void ConfigureMiddleware(WebApplication app, bool isDevelopment)
        {
            // 响应压缩
            app.UseResponseCompression();

            // 开发环境启用SwaggerUI
            if (isDevelopment)
            {
                app.UseCustomSwaggerUI(options => options.Title = SwaggerDocTitle);
            }

            // 路由
            app.UseRouting();

            // 认证授权
            app.UseAuthentication();
            app.UseAuthorization();

            // 配置文件上传路径
            //ConfigureUploadPath(app, isDevelopment);

            // API路由映射
            app.MapControllers();

            // 生产环境启用SPA
            if (!isDevelopment)
            {
                app.UseSpaStaticFiles();
                app.UseSpa(spa => spa.Options.SourcePath = SpaSourcePath);
            }
        }

        /// <summary>
        /// 初始化应用服务数据
        /// </summary>
        private static void InitApplicationServices(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                // 初始化用户
                var userService = services.GetRequiredService<UserService>();
                userService.InitUser();

                // 初始化Cookie
                var cookieService = services.GetRequiredService<DyCookieService>();
                cookieService.Init();

                // 初始化配置
                var commonService = services.GetRequiredService<CommonService>();
                var config = commonService.InitConfig();

                // 更新收藏视频类型--兼容老版本-原来的旧数据没有这个类型字段
                commonService.UpdateCollectViedoType();
                // 重置博主作品同步状态为未同步
                commonService.UpdateAllCookieSyncedToZero();
                // 启动定时任务
                var quartzJobService = services.GetRequiredService<QuartzJobService>();
                quartzJobService.StartJob(config?.Cron ?? "30");

                Serilog.Log.Debug("系统初始化完成，会默认将-博主作品同步功能-同步全部作品重置为关闭（若要开启，可以到抖音授权页面中修改）");
                Serilog.Log.Debug($"默认设置的每次读取行数为:{config.BatchCount}，可前往系统配置页修改");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Failed to initialize services on startup");
            }
        }

     

       
    }
}