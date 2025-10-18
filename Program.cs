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
            ConfigureJwtAuthentication(services);
        }

        /// <summary>
        /// 配置JWT认证
        /// </summary>
        private static void ConfigureJwtAuthentication(IServiceCollection services)
        {
            var key = Encoding.ASCII.GetBytes(Md5Util.JWT_TOKEN_KEY);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Headers["Authorization"]
                                               .FirstOrDefault()?.Split(" ").Last();
                        return Task.CompletedTask;
                    }
                };
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(60)
                };
            });
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
        /// 配置上传路径
        /// </summary>
        private static void ConfigureUploadPath(WebApplication app, bool isDevelopment)
        {
            var uploadPath = isDevelopment
                ? Md5Util.UPLOAD_PATH_DEV
                : Md5Util.UPLOAD_PATH_PRO;

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 如需启用文件上传访问可取消注释
            // app.UseStaticFiles(new StaticFileOptions
            // {
            //     FileProvider = new PhysicalFileProvider(uploadPath),
            //     RequestPath = "/upload"
            // });
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
                userService.InitUser(new LoginUserInfo
                {
                    UserName = "douyin",
                    Password = "douyin2025",
                    CreateTime = DateTime.Now
                });

                // 初始化Cookie
                var cookieService = services.GetRequiredService<DyCookieService>();
                cookieService.Init(new DyUserCookies
                {
                    UserName = "douyin",
                    Cookies = "--",
                    Id = "2026",
                    SavePath = "/app/collect",
                    Status = 0,
                    SecUserId = "--",
                    FavSavePath = "/app/favorite",
                    UpSavePath = "/app/uper",
                });



                // 初始化配置
                var commonService = services.GetRequiredService<CommonService>();
                var config = commonService.InitConfig(new AppConfig
                {
                    Id = IdGener.GetLong().ToString(),
                    Cron = "30",
                    BatchCount = 20
                });

                // 更新收藏视频类型--兼容老版本-原来的旧数据没有这个类型字段
                commonService.UpdateCollectViedoType();

                commonService.UpdateAllCookieSyncedToZero();

                // 启动定时任务
                var quartzJobService = services.GetRequiredService<QuartzJobService>();
                quartzJobService.StartJob(config?.Cron ?? "30");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Failed to initialize services on startup");
            }
        }

        /// <summary>
        /// 创建SQLite数据库连接字符串
        /// </summary>
        private static string CreateSqliteDBConn()
        {
            var dbFolder = Path.Combine(Environment.CurrentDirectory, "db");
            Directory.CreateDirectory(dbFolder); // 不存在则创建，无需判断

            var dbPath = Path.Combine(dbFolder, "dy.sqlite");
            if (!File.Exists(dbPath))
            {
                using (File.Create(dbPath)) { } // 使用using确保文件流关闭
            }

            return $"DataSource={dbPath}";
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private static ISqlSugarClient InitDataBase(DbType dbType, string connString)
        {
            // 处理SQLite连接字符串
            if (dbType == DbType.Sqlite)
            {
                connString = CreateSqliteDBConn();
            }

            if (string.IsNullOrWhiteSpace(connString))
            {
                return null;
            }

            return new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = connString,
                InitKeyType = InitKeyType.Attribute,
                DbType = dbType,
                IsAutoCloseConnection = true
            }, db =>
            {
                // 日志配置
                db.Aop.OnLogExecuting = (sql, pars) => Serilog.Log.Debug(sql);
                db.Aop.OnError = e =>
                {
                    Serilog.Log.Error(e.Message);
                    Serilog.Log.Error(e.Sql);
                };

                // 创建数据库和表
                db.DbMaintenance.CreateDatabase();
                var modelTypes = Assembly.GetExecutingAssembly()
                                         .GetTypes()
                                         .Where(t => t.Namespace?.StartsWith("dy.net.model") ?? false)
                                         .ToArray();
                db.CodeFirst.InitTables(modelTypes);
            });
        }
    }
}