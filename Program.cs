using dy.net.extension;
using dy.net.service;
using dy.net.utils;
using dy.sync.lib;
using Serilog;
using System.Reflection;
using System.Text;

namespace dy.net
{
    public class Program
    {
        // 常量定义
        private static string DefaultListenUrl = "10101";
        private const string SpaRootPath = "app/dist";
        private const string SpaSourcePath = "app/";
        private const string SwaggerDocTitle = "dy.net WebApi Docs";

        /// <summary>
        /// 
        /// </summary>
        public static void Main(string[] args)
        {
            // 初始化编码提供器
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // 构建Web应用
            var builder = WebApplication.CreateBuilder(args);
            var isDevelopment = builder.Environment.IsDevelopment();
            // 配置主机
            ConfigureHost(builder, isDevelopment);
            // 配置服务--数据库保存路径从命令行参数传入的第一个参数
            ConfigureServices(builder.Services, builder.Configuration, builder.Environment, args.Length > 0 ? args[0] : "");
            // 构建应用
            var app = builder.Build();
            // 配置中间件
            ConfigureMiddleware(app, builder.Environment);
            Log.Debug($"dy.sync app is started successfully  on  {DefaultListenUrl}");
            // 初始化应用服务
            InitApplicationServices(app, isDevelopment);
            Console.WriteLine();
         
            app.Run();
        }

        /// <summary>
        /// 配置主机设置
        /// </summary>
        private static void ConfigureHost(WebApplicationBuilder builder, bool isDevelopment)
        {
            InitAppsettings(builder);
            //from docker yaml file 环境变量 或者 dockerfile 或appsettings.json 
            DefaultListenUrl = $"http://*:{builder.Configuration.GetValue<string>(SystemStaticUtil.APP_PORT) ?? DefaultListenUrl}";
            // 设置监听地址
            builder.WebHost.UseUrls(DefaultListenUrl);
            // 配置日志
            builder.ConfigureLogging();
        }

        private static void InitAppsettings(WebApplicationBuilder builder)
        {
            // 配置配置文件
            builder.Host.ConfigureAppConfiguration((context, config) =>
            {
                // 定义配置文件名（统一小写，适配Linux大小写敏感特性）
                const string configFileName = "appsettings.json";

                // 步骤1：获取多维度的候选基础路径（覆盖不同部署场景）
                var candidateBasePaths = new List<string>
                {
                    // 候选1：程序集所在目录（优先，解决工作目录不一致问题）
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    // 候选2：当前工作目录（兜底）
                    Directory.GetCurrentDirectory(),
                    // 候选3：应用根目录（针对单文件发布/容器部署场景）
                    AppContext.BaseDirectory,
                    // 候选4：自定义环境变量指定的配置目录（灵活性扩展）
                    Environment.GetEnvironmentVariable("APP_CONFIG_DIR")
                }
                // 过滤空值和无效路径
                .Where(path => !string.IsNullOrEmpty(path) && Directory.Exists(path))
                .Distinct() // 去重
                .ToList();

                // 步骤2：遍历候选路径，查找存在的配置文件
                string configFilePath = null;
                foreach (var basePath in candidateBasePaths)
                {
                    var tempPath = Path.Combine(basePath, configFileName);
                    if (File.Exists(tempPath))
                    {
                        configFilePath = tempPath;
                        break; // 找到第一个存在的配置文件即可
                    }
                }

                // 步骤3：配置加载（增加容错和日志提示）
                if (!string.IsNullOrEmpty(configFilePath))
                {
                    // 找到配置文件，正常加载
                    var basePath = Path.GetDirectoryName(configFilePath);
                    config.SetBasePath(basePath)
                          .AddJsonFile(configFileName, optional: false, reloadOnChange: true)
                          .AddEnvironmentVariables();

                    // 可选：输出日志，确认配置文件加载路径（方便排查）
                    //Console.WriteLine($"成功加载配置文件：{configFilePath}");
                }
                else
                {
                    // 未找到配置文件，抛出明确异常（或根据需求调整为兜底逻辑）
                    throw new FileNotFoundException(
                        $"未找到配置文件 {configFileName}，已检查以下路径：{string.Join("; ", candidateBasePaths)}",
                        configFileName);
                }
            });
        }

        /// <summary>
        /// 配置依赖注入服务
        /// </summary>
        private static void ConfigureServices(IServiceCollection services, IConfiguration config, IWebHostEnvironment environment,string dbPath)
        {

            //打印logo
            //PrintApp();

            services.AddSingleton(new Appsettings (config));
            // 雪花ID生成器
            services.AddSnowFlakeId(options => options.WorkId = new Random().Next(1, 100));

            // MVC控制器+异常拦截器
            services.AddControllers().AddGlobalExceptionFilter();

            // HTTP客户端
            services.AddHttpClients();

            // 数据库
            services.AddSqlsugar(dbPath);

            // 定时任务
            services.AddQuartzService();

            // 仓储和服务注册
            services.AddServicesFromNamespace("dy.net.repository")
                    .AddServicesFromNamespace("dy.net.service");

            services.AddScoped<FFmpegHelper>();
        
            // SPA静态文件支持
            services.AddSpaStaticFiles(options => options.RootPath = SpaRootPath);

            // 开发环境启用Swagger
            //if (environment.IsDevelopment())
            //{
            //    services.AddSwagger();
            //}

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
            //if (environment.IsDevelopment())
            //{
            //    app.UseCustomSwaggerUI(options => options.Title = SwaggerDocTitle);
            //}

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
                userService.InitUser("douyin","douyin2026");
                var commonService = services.GetRequiredService<DouyinCommonService>();
                
                // 更新视频类型--兼容老版本--不再需要
                //commonService.UpdateCollectViedoType();
                // 重置博主作品同步状态为未同步
                commonService.UpdateAllCookieSyncedToZero();
                // 初始化配置
                var config = commonService.InitConfig();
                if (!isDevelopment)
                {
                    // 启动定时任务
                    var quartzJobService = services.GetRequiredService<DouyinQuartzJobService>();
                    quartzJobService.InitOrReStartAllJobs(config?.Cron <= 0 ? "30" : config.Cron.ToString());
                    DouyinHttpHelper.GetTenImage(Appsettings.Get("tagName"));//查询镜像版本
                }
                // 初始化Cookie
                var deploy= Appsettings.Get("deploy");
                if (deploy != null&&deploy=="docker")//docker环境直接初始化一个默认的配置
                {
                    var cookieService = services.GetRequiredService<DouyinCookieService>();
                    cookieService.InitCookie();
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to initialize services on startup");
            }
        }

     
        private static void PrintApp()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;

            // 步骤1：定义原始ASCII艺术字行（无缩进）
            List<string> originalArtLines = new List<string>
        {
            "     __                                    __ ",
            " ___/ /_ __  ___ __ _____  ____  ___  ___ / /_",
            "/ _  / // / (_-</ // / _ \\__/ / _ \\/ -_) __/",
            "\\_,_/\\_, (_)___/\\_, /_//_/\\__(_)_//_/\\__/\\__/ ",
            "    /___/      /___/                          "
        };

            // 步骤2：设置缩进字符数（可自由修改：4、8、10 等）
            int indentCount = 4;
            string indent = new string(' ', indentCount); // 生成对应数量的空格

            // 步骤3：给每行添加缩进
            List<string> indentedArtLines = new List<string>();
            foreach (var line in originalArtLines)
            {
                indentedArtLines.Add(indent + line); // 每行开头拼接缩进空格
            }

            // 步骤4：找到缩进后最长行的长度（避免越界）
            int maxLength = 0;
            foreach (var line in indentedArtLines)
            {
                if (line.Length > maxLength) maxLength = line.Length;
            }

            // 步骤5：按列推进打印（从左到右，包含缩进）
            for (int col = 0; col < maxLength; col++)
            {
                Console.SetCursorPosition(0, 0); // 重置光标到第一行开头

                for (int row = 0; row < indentedArtLines.Count; row++)
                {
                    // 定位到「当前列、当前行」（已包含缩进偏移）
                    Console.SetCursorPosition(col, row);

                    // 打印字符（无字符则补空格）
                    if (col < indentedArtLines[row].Length)
                    {
                        Console.Write(indentedArtLines[row][col]);
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }

                Thread.Sleep(20); // 列推进速度（可调整：10=快，50=慢）
            }

            // 打印完成后，光标移到艺术字下方
            Console.SetCursorPosition(0, indentedArtLines.Count);
            Console.WriteLine();
            Console.ResetColor();

        }

    }
}