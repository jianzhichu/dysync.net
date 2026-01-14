using dy.net.job;
using dy.net.service;
using dy.net.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;

//using Serilog.Formatting.Compact;
using SqlSugar;
//using Swashbuckle.AspNetCore.SwaggerGen;
//using Swashbuckle.AspNetCore.SwaggerUI;
using System.IO.Compression;
using System.Net.Http;
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

        public static string FnDataFolder = string.Empty;
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

        private static string CreateSqliteDBConn(string dbPath="")
        {
            string fileFloder= Path.Combine(Environment.CurrentDirectory, "db"); 
            if (!string.IsNullOrEmpty(dbPath))
            { 
                fileFloder= Path.Combine(dbPath, "db");
                FnDataFolder = Path.Combine(dbPath, "mp3");
                if ((!Directory.Exists(FnDataFolder)))
                {
                    Directory.CreateDirectory(FnDataFolder);
                }
            }
            else
            {
                if (Appsettings.Get("deploy") == "fn")
                {
                   Log.Error($"fn--dbpath,未正常获取到，请进Q群联系作者 759876963");
                     throw new Exception("fn--dbpath,未正常获取到，请进Q群联系作者 759876963");
                }
            }

            if (!Directory.Exists(fileFloder))
            {
                Directory.CreateDirectory(fileFloder);
            }
            var filePath = Path.Combine(fileFloder, "dy.sqlite");
            string conn = $"DataSource={filePath}";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();//.Close();
            }

            return conn;

        }


        /// <summary>
        /// 配置JWT认证
        /// </summary>
        public static void ConfigureJwtAuthentication(this IServiceCollection services)
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

        public static void AddSqlsugar(this IServiceCollection services,string dbpath)
        {
            //DbType dbtype = GetDBType(configuration);
            services.AddScoped<ISqlSugarClient>(db =>
            {
                var sqlSugar = new SqlSugarClient(new ConnectionConfig
                {
                    ConnectionString = CreateSqliteDBConn( dbpath),
                    InitKeyType = InitKeyType.Attribute,
                    DbType = DbType.Sqlite,
                    IsAutoCloseConnection = true // close connection after each operation (recommended)
                }, db =>
                {
                    //单例参数配置，所有上下文生效       
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        //Serilog.Log.Debug(sql);//输出sql
                    };

                    db.Aop.OnError = (e) =>
                    {
                        Serilog.Log.Error(e.Message);
                        Serilog.Log.Error(e.Sql);
                    };

                    db.DbMaintenance.CreateDatabase(); // 检查数据库是否存在，如果不存在则创建
                    Assembly assembly = Assembly.GetExecutingAssembly(); // 获取当前程序集
                    Type[] types = assembly.GetTypes(); // 获取程序集中的所有类型

                    var targetClasses = types
                        .Where(t => t.Namespace != null && t.Namespace.StartsWith("dy.net.model"))
                        .ToList();
                    db.CodeFirst.InitTables(targetClasses.ToArray());
                });

                return sqlSugar;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public static void AddQuartzService(this IServiceCollection services)
        {
            //services.AddTransient<DouyinCollectSyncJob>();
            //services.AddTransient<DouyinFavoritSyncJob>();
            //services.AddTransient<DouyinUperPostSyncJob>();

            // 注册Quartz服务
            services.AddQuartz();

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            services.AddTransient<DouyinQuartzJobService>();
        }


        //public static void AddHttpClients(this IServiceCollection services)
        //{
        //    services.AddHttpClient("dy_collect", client =>
        //    {
        //        client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2");
        //        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");
        //        client.DefaultRequestHeaders.Add("Referer", "https://www.douyin.com");
        //    });

        //    services.AddHttpClient("dy_follow", client =>
        //    {
        //        client.DefaultRequestHeaders.Referrer = new Uri("https://www.douyin.com/user/self?showTab=like");
        //    });

        //    services.AddHttpClient("dy_favorite", client =>
        //    {
        //        client.DefaultRequestHeaders.Referrer = new Uri("https://www.douyin.com/user/self?showTab=like");
        //    });

        //    services.AddHttpClient("dy_uper", client =>
        //    {
        //        client.DefaultRequestHeaders.Referrer = new Uri("https://www.douyin.com/user/MS4wLjABAAAA1zFIBhWG-3qS8MiggDMhyCpqDnvfGYf_mtsdgtBzV7A?from_tab_name=main&showTab=post&vid=7576282367263807451");
        //    });
        //    // 配置HttpClient（Startup.cs或Program.cs）
        //    services.AddHttpClient("dy_download")
        //        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        //        {
        //            // 控制并发连接数（根据服务器承受能力调整，建议5-20）
        //            MaxConnectionsPerServer = 5,
        //            // 禁用代理自动检测（减少不必要的延迟）
        //            UseProxy = false,
        //            // 连接超时（建立连接的超时时间）
        //            ConnectTimeout = TimeSpan.FromSeconds(60)
        //        })
        //        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        //        {
        //            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        //        })
        //        // 配置客户端默认请求头
        //        .ConfigureHttpClient(client =>
        //        {
        //            client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2");
        //            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");
        //            client.DefaultRequestHeaders.Add("Referer", "https://www.douyin.com");
        //        }); 
        //}



        public static void AddHttpClients(this IServiceCollection services)
        {
            // 通用的忽略SSL证书验证的HttpMessageHandler配置
            Func<HttpMessageHandler> ignoreSslHandlerFactory = () =>
            {
                var handler = new SocketsHttpHandler
                {
                    // 控制并发连接数（根据服务器承受能力调整，建议5-20）
                    MaxConnectionsPerServer = 5,
                    // 禁用代理自动检测（减少不必要的延迟）
                    UseProxy = false,
                    // 连接超时（建立连接的超时时间）
                    ConnectTimeout = TimeSpan.FromSeconds(60),
                    // 忽略HTTPS证书验证
                    SslOptions = new SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                    }
                };

                return handler;
            };

            // 抖音采集客户端
            services.AddHttpClient("dy_collect", client =>
            {
                client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Referer", "https://www.douyin.com");
            }).ConfigurePrimaryHttpMessageHandler(ignoreSslHandlerFactory);

            // 抖音关注客户端
            services.AddHttpClient("dy_follow", client =>
            {
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.douyin.com/user/self?showTab=like");
            }).ConfigurePrimaryHttpMessageHandler(ignoreSslHandlerFactory);

            // 抖音收藏客户端
            services.AddHttpClient("dy_favorite", client =>
            {
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.douyin.com/user/self?showTab=like");
            }).ConfigurePrimaryHttpMessageHandler(ignoreSslHandlerFactory);

            // 抖音UP主客户端
            services.AddHttpClient("dy_uper", client =>
            {
                client.DefaultRequestHeaders.Referrer = new Uri("https://www.douyin.com/user/MS4wLjABAAAA1zFIBhWG-3qS8MiggDMhyCpqDnvfGYf_mtsdgtBzV7A?from_tab_name=main&showTab=post&vid=7576282367263807451");
            }).ConfigurePrimaryHttpMessageHandler(ignoreSslHandlerFactory);

            // 抖音下载客户端（单独配置，保持原有特性）
            services.AddHttpClient("dy_download")
                .ConfigurePrimaryHttpMessageHandler(ignoreSslHandlerFactory)
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Referer", "https://www.douyin.com");
                });

            // 如果你需要兼容旧版.NET（使用HttpClientHandler而非SocketsHttpHandler），可以使用以下配置：
            // Func<HttpMessageHandler> legacyIgnoreSslHandlerFactory = () =>
            // {
            //     var handler = new HttpClientHandler
            //     {
            //         ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
            //         MaxConnectionsPerServer = 5,
            //         UseProxy = false,
            //         ConnectTimeout = TimeSpan.FromSeconds(60)
            //     };
            //     return handler;
            // };
        }

        /// <summary>
        /// 自动注入指定命名空间下的所有服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="namespace">目标命名空间（如 "MyApp.Services"）</param>
        /// <param name="assembly">服务所在的程序集（默认当前程序集）</param>
        /// <param name="includeSubNamespaces">是否包含子命名空间</param>
        public static IServiceCollection AddServicesFromNamespace(
            this IServiceCollection services,
            string @namespace,
            Assembly? assembly = null,
            bool includeSubNamespaces = false)
        {
            // 默认使用调用方的程序集
            assembly ??= Assembly.GetCallingAssembly();

            // 扫描程序集中指定命名空间的所有类
            var types = assembly.GetTypes()
                .Where(type =>
                    type.IsClass && // 只处理类
                    !type.IsAbstract && // 排除抽象类
                    !type.IsGenericTypeDefinition && // 排除泛型类
                    (includeSubNamespaces
                        ? type.Namespace?.StartsWith(@namespace, StringComparison.Ordinal) == true
                        : type.Namespace == @namespace) // 匹配命名空间（含子命名空间或精确匹配）
                );

            foreach (var type in types)
            {
                // 获取服务的生命周期（优先使用特性，默认 Transient）
                var lifetime = type.GetCustomAttribute<ServiceLifetimeAttribute>()?.Lifetime
                               ?? ServiceLifetime.Transient;

                // 查找该类实现的接口（优先注册为接口服务）
                //var interfaces = type.GetInterfaces()
                //    .Where(i => !i.IsGenericType || !i.GetGenericTypeDefinition().Equals(typeof(IDisposable)))
                //    .ToList();
                // 查找该类实现的接口（排除 IDisposable）
                var interfaces = type.GetInterfaces()
                    .Where(i => i != typeof(IDisposable)) // <-- 关键修正
                    .ToList();

                if (interfaces.Any())
                {
                    // 注册：接口 -> 实现类（如 IUserService -> UserService）
                    foreach (var @interface in interfaces)
                    {
                        services.Add(new ServiceDescriptor(@interface, type, lifetime));
                    }
                }
                else
                {
                    // 无接口时，直接注册类本身（如 UserService 注册为自身）
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
        public static void AddMyResponseCompression(this IServiceCollection services)
        {

            // 第一步: 配置gzip与br的压缩等级为最优
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            // 第二步: 添加中间件
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                // 添加br与gzip的Provider
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                // 扩展一些类型 (MimeTypes中有一些基本的类型,可以打断点看看)
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "text/html; charset=utf-8",
                    "application/xhtml+xml",
                    "application/atom+xml",
                    "image/svg+xml",
                    "application/octet-stream"
                });
            });
        }
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
