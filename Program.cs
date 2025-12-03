using dy.net.extension;
using dy.net.service;
using dy.net.utils;
using Serilog;
using System.Text;

namespace dy.net
{
    public class Program
    {
        // 常量定义
        private static string DefaultListenUrl = "http://*:10101";
        private const string SpaRootPath = "app/dist";
        private const string SpaSourcePath = "app/";
        private const string SwaggerDocTitle = "dy.net WebApi Docs";

        /// <summary>
        /// 打包时注意，如果是false,前端不允许修改开启下载图片和视频的选项
        /// </summary>
        public static void Main(string[] args)
        {
            //Console.ForegroundColor = ConsoleColor.Yellow;

            // 初始化编码提供器
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // 构建Web应用
            var builder = WebApplication.CreateBuilder(args);
            //from docker yaml file 环境变量 或者 dockerfile 或appsettings.json 
            DefaultListenUrl = builder.Configuration.GetValue<string>(SystemStaticUtil.ASPNETCORE_URLS) ?? DefaultListenUrl;
     
            var isDevelopment = builder.Environment.IsDevelopment();

            // 配置主机
            ConfigureHost(builder, isDevelopment);

            // 配置服务
            ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

            // 构建应用
            var app = builder.Build();
            Log.Debug("ffmpeg is on");

            // 配置中间件
            ConfigureMiddleware(app, builder.Environment);

            // 初始化应用服务
            InitApplicationServices(app, isDevelopment);

            Serilog.Log.Debug("dy.sync service is starting...");
            Log.Debug("dy.sync service is started successfully");
            Console.WriteLine("------------------------------------------------------------------------");
            Console.WriteLine(@" __ \\ \   /  ___|\ \   /  \  |  ___| 
 |   |\   / \___ \ \   /    \ | |     
 |   |   |        |   |   |\  | |     
____/   _|_)_____/   _|  _| \_|\____| 
                                      
");
            //Console.ResetColor();
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
        private static void ConfigureServices(IServiceCollection services, IConfiguration config, IWebHostEnvironment environment)
        {

            services.AddSingleton(new Appsettings (config));
            // 雪花ID生成器
            services.AddSnowFlakeId(options => options.WorkId = new Random().Next(1, 100));

            // MVC控制器
            services.AddControllers();

            // HTTP客户端
            services.AddHttpClients();

            // 数据库
            services.AddSqlsugar(config);

            // 定时任务
            services.AddQuartzService();

            // 仓储和服务注册
            services.AddServicesFromNamespace("dy.net.repository")
                    .AddServicesFromNamespace("dy.net.service");

            services.AddSingleton<FFmpegHelper>();
            ////下载图片合成视频-需要ffmpeg支持,镜像会很大。
            //if (downImageVideo)
            //{
            //    //根据配置动态加载dy.image程序集
            //    Assembly assembly = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "dy.image.dll"));
            //    services.AddServicesFromNamespace("dy.image", assembly);
            //}
            // SPA静态文件支持
            services.AddSpaStaticFiles(options => options.RootPath = SpaRootPath);

            // 开发环境启用Swagger
            if (environment.IsDevelopment())
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
        private static void ConfigureMiddleware(WebApplication app, IWebHostEnvironment environment)
        {
            // 响应压缩
            app.UseResponseCompression();

            // 开发环境启用SwaggerUI
            if (environment.IsDevelopment())
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

            app.UseStaticFiles();
            // 生产环境启用SPA
            if (!environment.IsDevelopment())
            {
                app.UseSpaStaticFiles();
                app.UseSpa(spa => spa.Options.SourcePath = SpaSourcePath);
            }
        }

        /// <summary>
        /// 初始化应用服务数据
        /// </summary>
        private static void InitApplicationServices(WebApplication app,bool isDevelopment)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                // 初始化用户
                var userService = services.GetRequiredService<AdminUserService>();
                userService.InitUser();

                // 初始化Cookie
                var cookieService = services.GetRequiredService<DouyinCookieService>();
                cookieService.InitCookie();

                // 初始化配置
                var commonService = services.GetRequiredService<DouyinCommonService>();
                var config = commonService.InitConfig();

                // 更新视频类型--兼容老版本
                commonService.UpdateCollectViedoType();
                // 重置博主作品同步状态为未同步
                commonService.UpdateAllCookieSyncedToZero();

                if (!isDevelopment)
                {
                    // 启动定时任务
                    var quartzJobService = services.GetRequiredService<DouyinQuartzJobService>();
                    quartzJobService.InitOrReStartAllJobs(config?.Cron <= 0 ? "30" : config.Cron.ToString());
                }

            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to initialize services on startup");
            }
        }

     

       
    }
}