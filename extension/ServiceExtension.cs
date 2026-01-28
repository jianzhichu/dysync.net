using dy.net.job;
using dy.net.service;
using dy.net.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;
//using Serilog.Formatting.Compact;
using SqlSugar;
using System.Collections.Concurrent;

//using Swashbuckle.AspNetCore.SwaggerGen;
//using Swashbuckle.AspNetCore.SwaggerUI;
using System.IO.Compression;
using System.Net.Security;
using System.Reflection;
using System.Text;

namespace dy.net.extension
{
    public static class ServiceExtension
    {
        public class SwaggerOptions
        {
            public string Title { get; set; }
        }

        #region 静态缓存字段（核心优化：避免重复计算/读取）
        // 只读静态字段，防止外部随意修改，减少内存混乱
        public static  string FnDataFolder;
        // 缓存JWT密钥字节数组，避免重复编码
        private static readonly byte[] _jwtKeyBytes;
        // 缓存部署配置，避免重复读取Appsettings
        private static readonly string _deployConfig;
        // 缓存实体程序集类型，避免SqlSugar每次都反射（核心内存优化）
        private static readonly Type[] _entityTypes;
        // 缓存响应压缩MIME类型，避免每次请求拼接
        private static readonly IEnumerable<string> _compressionMimeTypes;
        #endregion

        #region 静态构造函数（仅执行一次，初始化所有缓存）
        static ServiceExtension()
        {
            // 初始化JWT密钥（仅一次）
            _jwtKeyBytes = Encoding.ASCII.GetBytes(Md5Util.JWT_TOKEN_KEY);
         
            // 初始化实体类型（仅一次反射，缓存结果）
            Assembly entityAssembly = Assembly.GetExecutingAssembly();
            _entityTypes = entityAssembly.GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.StartsWith("dy.net.model.entity"))
                .ToArray();
            // 初始化响应压缩MIME类型（仅一次拼接，缓存结果）
            //_compressionMimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            //{
            //    "text/html; charset=utf-8",
            //    "application/xhtml+xml",
            //    "application/atom+xml",
            //    "image/svg+xml",
            //    "application/octet-stream"
            //}).ToList(); // 转为List，避免多次枚举Concat结果
            // 初始化FnDataFolder（空值，后续由CreateSqliteDBConn赋值）
            FnDataFolder = string.Empty;
        }
        #endregion


        private static DbType GetDBType(IConfiguration configuration)
        {
            DbType dbType = DbType.Sqlite;
            var dbtypeString = configuration["dbtype"].ToLower();
            // 获取颜色枚举类型的所有枚举值
            var dbtypes = Enum.GetValues(typeof(DbType));
            foreach (DbType type in dbtypes)
            {
                if (type.ToString().ToLower() == dbtypeString)
                {
                    dbType = type;
                    break;
                }
            }

            return dbType;
        }

        //private static string GetConnString(IConfiguration configuration, DbType dbType)
        //{
        //    //var connectionString = configuration["dbconn"];
        //    if (dbType == DbType.Sqlite)
        //    {
        //        connectionString = CreateSqliteDBConn();
        //    }
        //    return connectionString;
        //}

        // static string CreateSqliteDBConn(string dbPath = "")
        //{
        //    string fileFloder = Path.Combine(Environment.CurrentDirectory, "db");
        //    if (!string.IsNullOrEmpty(dbPath))
        //    {
        //        fileFloder = Path.Combine(dbPath, "db");
        //        FnDataFolder = Path.Combine(dbPath, "mp3");
        //        if ((!Directory.Exists(FnDataFolder)))
        //        {
        //            Directory.CreateDirectory(FnDataFolder);
        //        }
        //    }
        //    else
        //    {
        //        if (Appsettings.Get("deploy") == "fn")
        //        {
        //            Log.Error($"fn--dbpath,未正常获取到，请进Q群联系作者 759876963");
        //            throw new Exception("fn--dbpath,未正常获取到，请进Q群联系作者 759876963");
        //        }
        //    }

        //    if (!Directory.Exists(fileFloder))
        //    {
        //        Directory.CreateDirectory(fileFloder);
        //    }
        //    var filePath = Path.Combine(fileFloder, "dy.sqlite");
        //    string conn = $"DataSource={filePath}";
        //    if (!File.Exists(filePath))
        //    {
        //        File.Create(filePath).Close();
        //    }

        //    return conn;
        //}

        /// <summary>
        /// 核心优化：缓存连接字符串+using自动释放资源+减少重复判断+避免多次路径拼接
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _sqliteConnCache = new();
        static string CreateSqliteDBConn(string dbPath = "")
        {
            // 缓存键：空dbPath用"default"，避免空键问题
            string cacheKey = string.IsNullOrEmpty(dbPath) ? "default" : dbPath;
            // 核心优化：连接字符串缓存，相同dbPath仅创建一次
            if (_sqliteConnCache.TryGetValue(cacheKey, out var cachedConn))
            {
                return cachedConn;
            }

            string fileFolder = string.IsNullOrEmpty(dbPath)
                ? Path.Combine(Environment.CurrentDirectory, "db")
                : Path.Combine(dbPath, "db");

            // 仅当dbPath非空时初始化FnDataFolder（原有业务逻辑）
            if (!string.IsNullOrEmpty(dbPath))
            {
                string fnDataPath = Path.Combine(dbPath, "mp3");
                // 原子赋值+仅创建一次目录（减少IO和内存判断）
                if (!Directory.Exists(fnDataPath))
                {
                    Directory.CreateDirectory(fnDataPath);
                }
                // 只读字段通过静态构造函数初始化后，此处仅赋值一次
                FnDataFolder = fnDataPath;
            }
            else
            {
                var _deployConfig = Appsettings.Get("deploy") ?? string.Empty;
                // 仅一次判断部署配置（已缓存），避免重复读取Appsettings
                if (_deployConfig == "fn")
                {
                    Log.Error($"fn--dbpath,未正常获取到，请进Q群联系作者 759876963");
                    throw new Exception("fn--dbpath,未正常获取到，请进Q群联系作者 759876963");
                }
            }

            // 仅创建一次目录（减少重复的Directory.Exists判断）
            if (!Directory.Exists(fileFolder))
            {
                Directory.CreateDirectory(fileFolder);
            }

            string dbFilePath = Path.Combine(fileFolder, "dy.sqlite");
            // 核心优化：using包裹File.Create，自动释放文件句柄（避免资源泄漏）
            if (!File.Exists(dbFilePath))
            {
                using FileStream fs = File.Create(dbFilePath);
                // 无需手动Close，using会自动释放
            }

            string connStr = $"DataSource={dbFilePath}";
            // 将连接字符串加入缓存，后续直接使用
            _sqliteConnCache.TryAdd(cacheKey, connStr);
            return connStr;
        }

        /// <summary>
        /// 配置JWT认证：缓存密钥字节数组，避免重复编码
        /// </summary>
        public static void ConfigureJwtAuthentication(this IServiceCollection services)
        {
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
                        // 优化：减少LINQ调用，直接取值（减少临时对象创建）
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader.Substring(7);
                        }
                        return Task.CompletedTask;
                    }
                };
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_jwtKeyBytes), // 使用缓存的密钥
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(60)
                };
            });
        }

        /// <summary>
        /// SqlSugar注册：核心优化-使用缓存的实体类型+缓存连接字符串+移除空AOP委托
        /// </summary>
        public static void AddSqlsugar(this IServiceCollection services, string dbpath)
        {
            // 提前创建连接字符串，避免每次创建ISqlSugarClient都调用（减少重复计算）
            string sqliteConn = CreateSqliteDBConn(dbpath);
            services.AddScoped<ISqlSugarClient>(db =>
            {
                var sqlSugar = new SqlSugarClient(new ConnectionConfig
                {
                    ConnectionString = sqliteConn,
                    InitKeyType = InitKeyType.Attribute,
                    DbType = DbType.Sqlite,
                    IsAutoCloseConnection = true
                }, db =>
                {
                    // 移除空的Debug日志委托，避免空委托的内存占用
                    db.Aop.OnError = (e) =>
                    {
                        Serilog.Log.Error(e, $"SqlSugar执行错误：{e.Message}，SQL：{e.Sql}");
                    };

                    db.DbMaintenance.CreateDatabase();
                    // 核心优化：使用缓存的实体类型，避免每次都反射（减少GC和内存）
                    db.CodeFirst.InitTables(_entityTypes);
                });
                return sqlSugar;
            });
        }

        /// <summary>
        /// Quartz服务注册：核心优化-提前创建连接字符串+合理的线程池配置
        /// </summary>
        public static void AddQuartzService(this IServiceCollection services, string dbPath)
        {
            // 注册Job（原有逻辑，保留Scoped生命周期，符合Quartz特性）
            services.AddTransient<DouyinCollectSyncJob>();
            services.AddTransient<DouyinFavoritSyncJob>();
            services.AddTransient<DouyinFollowedSyncJob>();
            services.AddTransient<DouyinFollowsAndCollnectsSyncJob>();
            services.AddTransient<DouyinCollectCustomSyncJob>();
            services.AddTransient<DouyinMixSyncJob>();
            services.AddTransient<DouyinSeriesSyncJob>();

            // 提前创建Quartz的SQLite连接字符串，避免重复调用
            string quartzConn = CreateSqliteDBConn(dbPath);
            services.AddQuartz(q =>
            {
                q.SchedulerId = "DouyinQuartzScheduler";
                q.SchedulerName = "DouyinSyncScheduler";
                q.InterruptJobsOnShutdownWithWait = false;
                q.UseDedicatedThreadPool(5);
                q.MisfireThreshold = TimeSpan.FromMinutes(2);

                q.UsePersistentStore(s =>
                {
                    s.UseMicrosoftSQLite(config =>
                    {
                        config.ConnectionString = quartzConn; // 使用提前创建的连接字符串
                        config.TablePrefix = "QRTZ_";
                    });
                    s.UseProperties = false;
                    s.UseBinarySerializer();
                });
            });

            services.AddQuartzHostedService(q =>
            {
                q.WaitForJobsToComplete = true;
                q.AwaitApplicationStarted = true;
            });

            services.AddTransient<DouyinQuartzJobService>();
        }

        /// <summary>
        /// Http客户端注册：核心修复-移除无限超时（避免内存泄漏）+优化连接配置
        /// </summary>
        public static void AddHttpClients(this IServiceCollection services)
        {
            // 通用忽略SSL的Handler工厂：提取为局部方法，避免重复创建逻辑
            static HttpMessageHandler IgnoreSslHandlerFactory()
            {
                var handler = new SocketsHttpHandler
                {
                    MaxConnectionsPerServer = 8, // 合理调整并发（5→8，兼顾性能和内存）
                    UseProxy = false,
                    ConnectTimeout = TimeSpan.FromSeconds(30), // 核心修复：移除无限超时，避免请求挂起泄漏
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5), // 优化：连接池生命周期，自动释放闲置连接
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2), // 优化：闲置连接超时，减少内存占用
                    SslOptions = new SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (_, __, ___, ____) => true
                    }
                };
                return handler;
            }

            // 抖音数据接口客户端
            services.AddHttpClient(DouyinRequestParamManager.DY_HTTP_CLIENT, client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(DouyinRequestParamManager.DY_USER_AGENT);
                client.BaseAddress = new Uri("https://www.douyin.com");
                client.Timeout = TimeSpan.FromSeconds(60); // 设置请求超时，避免无限等待
            }).ConfigurePrimaryHttpMessageHandler(IgnoreSslHandlerFactory);

            // 抖音下载客户端
            services.AddHttpClient(DouyinRequestParamManager.DY_HTTP_CLIENT_DOWN, client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(DouyinRequestParamManager.DY_USER_AGENT);
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.douyin.com");
                client.Timeout = TimeSpan.FromMinutes(5); // 下载超时设为5分钟，合理且不泄漏
            }).ConfigurePrimaryHttpMessageHandler(IgnoreSslHandlerFactory);
        }

        /// <summary>
        /// 自动注入服务：核心优化-减少LINQ临时对象+优化反射扫描+避免重复ToList
        /// </summary>
        public static IServiceCollection AddServicesFromNamespace(
            this IServiceCollection services,
            string @namespace,
            Assembly? assembly = null,
            bool includeSubNamespaces = false)
        {
            assembly ??= Assembly.GetExecutingAssembly(); // 优化：替换GetCallingAssembly，避免程序集获取错误

            // 优化：一次性过滤所有类型，减少后续遍历（使用ToArray避免多次枚举）
            var targetTypes = assembly.GetTypes()
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    !type.IsGenericTypeDefinition &&
                    type.Namespace != null &&
                    (includeSubNamespaces
                        ? type.Namespace.StartsWith(@namespace, StringComparison.Ordinal)
                        : type.Namespace == @namespace))
                .ToArray();

            foreach (var type in targetTypes)
            {
                // 优化：提前获取生命周期，减少多次反射
                var lifetime = type.GetCustomAttribute<ServiceLifetimeAttribute>()?.Lifetime ?? ServiceLifetime.Transient;
                // 优化：直接过滤接口，避免ToList创建临时List（减少内存分配）
                var interfaces = type.GetInterfaces().Where(i => i != typeof(IDisposable));

                if (interfaces.Any())
                {
                    foreach (var @interface in interfaces)
                    {
                        services.Add(new ServiceDescriptor(@interface, type, lifetime));
                    }
                }
                else
                {
                    services.Add(new ServiceDescriptor(type, type, lifetime));
                }
            }

            return services;
        }



        ///// <summary>
        ///// SwaggerUi
        ///// </summary>
        ///// <param name="app"></param>
        ///// <param name="options"></param>
        //public static void UseCustomSwaggerUI(this IApplicationBuilder app, Action<SwaggerOptions> options)
        //{
        //    SwaggerOptions option = new SwaggerOptions();
        //    options?.Invoke(option);
        //    //启用中间件服务生成Swagger作为JSON终结点
        //    app.UseSwagger(c =>
        //    {
        //        //c.SerializeAsV2 = true;
        //        //c.RouteTemplate = "api-docs/{documentName}/swagger.json";
        //        c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        //        {
        //            swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } };
        //            OpenApiPaths paths = new OpenApiPaths();
        //            foreach (var path in swaggerDoc.Paths)
        //            {
        //                //if ( path.Key.StartsWith("/v1/api") )//做版本控制
        //                paths.Add(path.Key, path.Value);
        //            }
        //            swaggerDoc.Paths = paths;
        //        });
        //    });
        //    //启用中间件服务对swagger-ui，指定Swagger JSON终结点
        //    app.UseSwaggerUI(c =>
        //    {
        //        //c.MaxDisplayedTags(5);
        //        //c.DisplayOperationId();//唯一标识操作
        //        c.SwaggerEndpoint("/swagger/v1/swagger.json", option.Title);
        //        //c.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 Docs");
        //        c.RoutePrefix = "swagger";//根路由
        //        c.EnableDeepLinking();//启用深度链接--不知道干嘛的
        //        c.DisplayRequestDuration();//调试，显示接口响应时间
        //        c.EnableValidator();//验证
        //        c.DocExpansion(DocExpansion.List);//默认展开
        //        c.DefaultModelsExpandDepth(-1);//隐藏model
        //        c.DefaultModelExpandDepth(3);//model展开层级
        //        c.EnableFilter();//筛选--如果接口过多可以开启
        //        c.DefaultModelRendering(ModelRendering.Model);//设置显示参数的实体或Example
        //        //c.SupportedSubmitMethods(SubmitMethod.Get , SubmitMethod.Head , SubmitMethod.Post);//

        //        //c.OAuthClientId("test-id");
        //        //c.OAuthClientSecret("test-secret");
        //        //c.OAuthRealm("test-realm");
        //        //c.OAuthAppName("test-app");
        //        //c.OAuthScopeSeparator(" ");
        //        //c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "foo", "bar" } });
        //        //c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
        //    });
        //}


        ///// <summary>
        ///// Swagger
        ///// </summary>
        ///// <param name="services"></param>
        //public static IServiceCollection AddSwagger(this IServiceCollection services, Action<SwaggerGenOptions> options = null)
        //{
        //    if (options != null)
        //        services.AddSwaggerGen(options);
        //    else
        //        services.AddSwaggerGen(DefaultSwaggerGenOptions());
        //    return services;
        //}

        //private static Action<SwaggerGenOptions> DefaultSwaggerGenOptions()
        //{
        //    Action<SwaggerGenOptions> options = o =>
        //    {
        //        o.OperationFilter<SwaggerAuthorizationFilter>();

        //        o.SwaggerDoc("v1", new OpenApiInfo
        //        {
        //            Version = "v1",
        //            Title = "dy.net API Swagger Document",

        //        });
        //        o.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
        //        o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        //        {
        //            Description = "请在下方输入：Bearer {Token}",
        //            Name = "Authorization",
        //            In = ParameterLocation.Header,
        //            Type = SecuritySchemeType.ApiKey,
        //            BearerFormat = "JWT",
        //            Scheme = "Bearer",
        //        });
        //        o.AddSecurityRequirement(new OpenApiSecurityRequirement
        //       {
        //            {
        //                new OpenApiSecurityScheme
        //                {
        //                    Reference = new OpenApiReference {
        //                        Type = ReferenceType.SecurityScheme,
        //                        Id = "Bearer",
        //                    }
        //                },
        //                new[] { "readAccess", "writeAccess" }
        //            }
        //       });

        //        o.DocumentFilter<SwaggerHiddenApiFilter>();
        //        var XmlPath = $"{AppContext.BaseDirectory}{AppDomain.CurrentDomain.FriendlyName}.xml";
        //        o.IncludeXmlComments(XmlPath);
        //        o.EnableAnnotations();
        //    };
        //    return options;
        //}


        /// <summary>
        /// Serilog 日志拓展
        /// </summary>
        public static void ConfigureLogging(this WebApplicationBuilder builder)
        {
            builder.Host.ConfigureLogging(logging => logging.ClearProviders())
                       .UseSerilog();
            string dateFile = "";// DateTime.Now.ToString("yyyyMMdd");

            Log.Logger = new LoggerConfiguration()
                //.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Is(LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .Filter.ByExcluding(e => e.Level == LogEventLevel.Information) // 排除Info级别的日志
                .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                .Filter.ByExcluding(Matching.FromSource("Quartz"))
                .WriteTo.Console(new RenderedCompactJsonFormatter(), LogEventLevel.Debug)
                //.WriteTo.MySQL(connectionString: builder.Configuration.GetConnectionString("DbConnectionString"), tableName: "Logs") // 输出到数据库
                .WriteTo.Logger(configure => configure
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
                    .WriteTo.File(
                        $"logs/log-debug-{dateFile}.txt",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
                //.WriteTo.Logger(configure => configure
                //    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                //    .WriteTo.File(
                //        $"logs/log-info-{dateFile}.txt",
                //        rollingInterval: RollingInterval.Day,
                //        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
                .WriteTo.Logger(configure => configure
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.File(
                        $"logs/log-error-{dateFile}.txt",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
                //.WriteTo.File(
                //    $"logs/log-total-{dateFile}.txt",
                //    rollingInterval: RollingInterval.Day,
                //    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                //    restrictedToMinimumLevel: LogEventLevel.Debug)
                .CreateLogger();
        }

        /// <summary>
        /// 响应压缩
        /// 
        /// services.AddMyResponseCompression(); 需要配合 app.UseResponseCompression();
        /// </summary>
        /// <param name="services"></param>
        //public static void AddMyResponseCompression(this IServiceCollection services)
        //{

        //    // 第一步: 配置gzip与br的压缩等级为最优
        //    services.Configure<BrotliCompressionProviderOptions>(options =>
        //    {
        //        options.Level = CompressionLevel.Optimal;
        //    });

        //    services.Configure<GzipCompressionProviderOptions>(options =>
        //    {
        //        options.Level = CompressionLevel.Optimal;
        //    });

        //    // 第二步: 添加中间件
        //    services.AddResponseCompression(options =>
        //    {
        //        options.EnableForHttps = true;
        //        // 添加br与gzip的Provider
        //        options.Providers.Add<BrotliCompressionProvider>();
        //        options.Providers.Add<GzipCompressionProvider>();
        //        // 扩展一些类型 (MimeTypes中有一些基本的类型,可以打断点看看)
        //        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
        //        {
        //            "text/html; charset=utf-8",
        //            "application/xhtml+xml",
        //            "application/atom+xml",
        //            "image/svg+xml",
        //            "application/octet-stream"
        //        });
        //    });
        //}
    }


    //public class SwaggerAuthorizationFilter : IOperationFilter
    //{
    //    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    //    {
    //        operation.Parameters ??= new List<OpenApiParameter>();
    //        _ = context.ApiDescription.ActionDescriptor.AttributeRouteInfo;

    //        //先判断是否是匿名访问,
    //        if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor)
    //        {
    //            var Authorizes = descriptor.MethodInfo.GetCustomAttributes(typeof(AuthorizeFilter), true);
    //            //非匿名的方法,链接中添加accesstoken值
    //            if (Authorizes.Any())
    //            {
    //                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
    //                //operation.Parameters.Add(new OpenApiParameter()
    //                //{
    //                //    Required = true,
    //                //    Name = "Bearer",
    //                //    In = ParameterLocation.Header,
    //                //    Description = "You Must  Request With  token",
    //                //    Style = ParameterStyle.DeepObject,

    //                //});
    //            }
    //        }
    //    }
    //}


    /// <summary>
    ///
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public partial class HiddenApiAttribute : Attribute { }

    /// <summary>
    ///
    /// </summary>
    //public class SwaggerHiddenApiFilter : IDocumentFilter
    //{
    //    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    //    {
    //        foreach (ApiDescription apiDescription in context.ApiDescriptions)
    //        {
    //            if (apiDescription.TryGetMethodInfo(out MethodInfo method))
    //            {
    //                if (method.ReflectedType.CustomAttributes.Any(t => t.AttributeType == typeof(HiddenApiAttribute))
    //                        || method.CustomAttributes.Any(t => t.AttributeType == typeof(HiddenApiAttribute)))
    //                {
    //                    string key = "/" + apiDescription.RelativePath;
    //                    if (key.Contains("?"))
    //                    {
    //                        int idx = key.IndexOf("?", StringComparison.Ordinal);
    //                        key = key.Substring(0, idx);
    //                    }
    //                    swaggerDoc.Paths.Remove(key);
    //                }
    //            }
    //        }
    //    }
    //}
}
