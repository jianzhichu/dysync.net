using dy.net.job;
using dy.net.model.dto;
using dy.net.model.entity;
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
using Serilog.Configuration;
using Serilog.Core;
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
        // 缓存部署配置，避免重复读取Appsettings
        private static readonly string _deployConfig;
        // 缓存实体程序集类型，避免SqlSugar每次都反射（核心内存优化）
        private static readonly Type[] _entityTypes;
        // CodeFirst 只允许在应用启动阶段执行一次，避免每个请求重复建表/改表。
        private static readonly SemaphoreSlim _databaseInitLock = new(1, 1);
        private static int _databaseInitialized;
        // 缓存响应压缩MIME类型，避免每次请求拼接
        private static readonly IEnumerable<string> _compressionMimeTypes;
        #endregion

        #region 静态构造函数（仅执行一次，初始化所有缓存）
        static ServiceExtension()
        {
            // 业务表使用显式白名单，绝不把 Qrtz* 实体交给 SqlSugar CodeFirst。
            _entityTypes = BusinessEntityRegistry.Types;
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
        /// 配置 JWT 认证。签名密钥由 JwtTokenService 统一管理，
        /// 应用重启后不会再随机变化。
        /// </summary>
        public static void ConfigureJwtAuthentication(
            this IServiceCollection services,
            JwtTokenService jwtTokenService)
        {
            ArgumentNullException.ThrowIfNull(jwtTokenService);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = jwtTokenService.CreateValidationParameters();
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(authHeader) &&
                            authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader.Substring(7).Trim();
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.Headers["X-Auth-Error"] =
                            context.Exception is SecurityTokenExpiredException
                                ? "TOKEN_EXPIRED"
                                : "TOKEN_INVALID";

                        Log.Warning(
                            context.Exception,
                            "JWT认证失败，Path={Path}，ErrorType={ErrorType}",
                            context.Request.Path,
                            context.Response.Headers["X-Auth-Error"].ToString());

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        if (!context.Response.Headers.ContainsKey("X-Auth-Error"))
                        {
                            context.Response.Headers["X-Auth-Error"] = "AUTH_REQUIRED";
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        }

        /// <summary>
        /// 注册 SqlSugar。这里只创建请求级客户端，不再执行 CodeFirst。
        /// CodeFirst 由 InitializeDatabaseAsync 在应用启动时统一执行一次。
        /// </summary>
        public static void AddSqlsugar(
            this IServiceCollection services,
            DatabaseConfigurationService databaseConfiguration)
        {
            var settings = databaseConfiguration.GetActiveSettings();

            services.AddScoped<ISqlSugarClient>(_ =>
            {
                return new SqlSugarClient(
                    databaseConfiguration.CreateConnectionConfig(settings), db =>
                {
                    db.Aop.OnError = e =>
                    {
                        Log.Error(e, "SqlSugar执行错误：{Message}，SQL：{Sql}", e.Message, e.Sql);
                    };
                });
            });
        }

        /// <summary>
        /// 应用启动时初始化数据库一次。
        /// 避免每次解析 ISqlSugarClient 都运行 CreateDatabase/CodeFirst，
        /// 解决连续刷新时并发 DDL、SQLite 锁和请求异常问题。
        /// </summary>
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            if (Volatile.Read(ref _databaseInitialized) == 1)
            {
                return;
            }

            await _databaseInitLock.WaitAsync();
            try
            {
                if (_databaseInitialized == 1)
                {
                    return;
                }

                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
                var databaseConfiguration = scope.ServiceProvider
                    .GetRequiredService<DatabaseConfigurationService>();
                var databaseSettings = databaseConfiguration.GetActiveSettings();

                Log.Information("开始执行数据库初始化和 CodeFirst，共 {EntityCount} 个实体", _entityTypes.Length);
                databaseConfiguration.EnsureDatabaseAvailable(db, databaseSettings);
                db.CodeFirst.InitTables(_entityTypes);
                // Quartz runtime tables use its official ADO schema, not business CodeFirst.
                QuartzSchemaInitializer.EnsureCreated(db);
                // CodeFirst 完成后补建业务索引。
                // 使用 IF NOT EXISTS，已有数据库和重复启动都安全。
                EnsureDouyinVideoIndexes(db);
                // 首次升级时从视频表聚合一次；之后所有视频写入均事务性维护统计表。
                var videoStatisticsService = scope.ServiceProvider
                    .GetRequiredService<DouyinVideoStatisticsService>();
                await videoStatisticsService.EnsureInitializedAsync();
                Volatile.Write(ref _databaseInitialized, 1);
                Log.Information("数据库初始化和 CodeFirst 执行完成");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "数据库初始化或 CodeFirst 执行失败，应用停止启动");
                throw;
            }
            finally
            {
                _databaseInitLock.Release();
            }
        }

        /// <summary>
        /// Quartz服务注册：按当前业务数据库选择对应的持久化驱动。
        /// </summary>
        public static void AddQuartzService(
            this IServiceCollection services,
            DatabaseConfigurationService databaseConfiguration)
        {
            // 注册Job（原有逻辑，保留Scoped生命周期，符合Quartz特性）
            services.AddScoped<DouyinCollectSyncJob>();
            services.AddScoped<DouyinFavoritSyncJob>();
            services.AddScoped<DouyinFollowedSyncJob>();
            services.AddScoped<DouyinFollowsAndCollnectsSyncJob>();
            services.AddScoped<DouyinCollectCustomSyncJob>();
            services.AddScoped<DouyinMixSyncJob>();
            services.AddScoped<DouyinSeriesSyncJob>();

            var databaseSettings = databaseConfiguration.GetActiveSettings();
            services.AddQuartz(q =>
            {
                q.SchedulerId = "DouyinQuartzScheduler";
                q.SchedulerName = "DouyinSyncScheduler";
                q.InterruptJobsOnShutdownWithWait = false;
                q.UseDedicatedThreadPool(5);
                q.MisfireThreshold = TimeSpan.FromMinutes(2);

                q.UsePersistentStore(store =>
                {
                    store.UseProperties = false;
                    store.PerformSchemaValidation = true;
                    store.UseBinarySerializer();

                    switch (databaseSettings.DbType)
                    {
                        case DatabaseKinds.MySql:
                            store.UseMySqlConnector(options =>
                            {
                                options.ConnectionString = databaseSettings.ConnectionString;
                                options.TablePrefix = "QRTZ_";
                            });
                            break;

                        case DatabaseKinds.PostgreSql:
                            store.UsePostgres(options =>
                            {
                                options.ConnectionString = databaseSettings.ConnectionString;
                                options.TablePrefix = "QRTZ_";
                            });
                            break;

                        default:
                            store.UseMicrosoftSQLite(options =>
                            {
                                options.ConnectionString = databaseSettings.ConnectionString;
                                options.TablePrefix = "QRTZ_";
                            });
                            break;
                    }
                });
            });

            services.AddQuartzHostedService(q =>
            {
                q.WaitForJobsToComplete = true;
                q.AwaitApplicationStarted = true;
            });

            services.AddScoped<DouyinQuartzJobService>();
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
                client.BaseAddress = new Uri(DouyinRequestParamManager.DouyinHost);
                client.Timeout = TimeSpan.FromSeconds(60); // 设置请求超时，避免无限等待
            }).ConfigurePrimaryHttpMessageHandler(IgnoreSslHandlerFactory);

            // 抖音下载客户端
            services.AddHttpClient(DouyinRequestParamManager.DY_HTTP_CLIENT_DOWN, client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(DouyinRequestParamManager.DY_USER_AGENT);
                client.DefaultRequestHeaders.Referrer = new Uri(DouyinRequestParamManager.DouyinHost);
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
                    // 尊重显式注册，避免把带运行时构造参数的服务再次注册为 Transient。
                    !services.Any(descriptor => descriptor.ServiceType == type) &&
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

        /// <summary>
        /// 启动时为 dy_collect_video 补建常用查询索引。
        /// 仅由 InitializeDatabaseAsync 调用，一个进程最多执行一次。
        /// </summary>
        private static void EnsureDouyinVideoIndexes(
            ISqlSugarClient db)
        {
            var isMySql = db.CurrentConnectionConfig.DbType == DbType.MySql;
            var isPostgreSql = db.CurrentConnectionConfig.DbType == DbType.PostgreSQL;
            var quote = isMySql ? "`" : "\"";
            var ifNotExists = isMySql ? string.Empty : "IF NOT EXISTS ";
            // SqlSugar PostgreSQL CodeFirst normalizes unconfigured entity column names
            // to lower case. Quoted PascalCase identifiers therefore address different,
            // non-existent columns (for example "AwemeId" instead of "awemeid").
            string Q(string name)
            {
                var identifier = isPostgreSql ? name.ToLowerInvariant() : name;
                return $"{quote}{identifier}{quote}";
            }

            (string Name, string Sql)[] indexes =
            {
                // 按作品 ID 查询、去重和批量存在性判断。
                ("idx_dy_collect_video_aweme_id", $"CREATE INDEX {ifNotExists}{Q("idx_dy_collect_video_aweme_id")} ON {Q("dy_collect_video")} ({Q("AwemeId")})"),

                // 默认分页、最近记录和同步日期范围查询。
                ("idx_dy_collect_video_sync_time", $"CREATE INDEX {ifNotExists}{Q("idx_dy_collect_video_sync_time")} ON {Q("dy_collect_video")} ({Q("SyncTime")} DESC)"),

                // 按视频类型筛选并按同步时间倒序。
                ("idx_dy_collect_video_type_sync", $"CREATE INDEX {ifNotExists}{Q("idx_dy_collect_video_type_sync")} ON {Q("dy_collect_video")} ({Q("ViedoType")}, {Q("SyncTime")} DESC)"),

                // 按账号筛选并按同步时间倒序。
                ("idx_dy_collect_video_cookie_sync", $"CREATE INDEX {ifNotExists}{Q("idx_dy_collect_video_cookie_sync")} ON {Q("dy_collect_video")} ({Q("CookieId")}, {Q("SyncTime")} DESC)"),

                // 同时按视频类型、账号筛选。
                ("idx_dy_collect_video_type_cookie_sync", $"CREATE INDEX {ifNotExists}{Q("idx_dy_collect_video_type_cookie_sync")} ON {Q("dy_collect_video")} ({Q("ViedoType")}, {Q("CookieId")}, {Q("SyncTime")} DESC)"),

                // 收藏夹、合集、短剧的记录数量统计。
                ("idx_dy_collect_video_category", $"CREATE INDEX {ifNotExists}{Q("idx_dy_collect_video_category")} ON {Q("dy_collect_video")} ({Q("CateId")}, {Q("CateXId")}, {Q("ViedoType")})"),

                // 关注视频重复标题编号查询。
                ("idx_dy_collect_video_author_title_time", $"CREATE INDEX {ifNotExists}{Q("idx_dy_collect_video_author_title_time")} ON {Q("dy_collect_video")} ({Q("AuthorId")}, {Q("ViedoType")}, {Q("VideoTitleSimplify")}, {Q("CreateTime")} DESC)"),

                // 按博主用户 ID 查询视频。
                ("idx_dy_collect_video_dy_user_id", $"CREATE INDEX {ifNotExists}{Q("idx_dy_collect_video_dy_user_id")} ON {Q("dy_collect_video")} ({Q("DyUserId")})")
            };

            foreach (var index in indexes)
            {
                if (isMySql &&
                    DatabaseIndexHelper.MySqlIndexExists(
                        db,
                        "dy_collect_video",
                        index.Name))
                {
                    continue;
                }

                db.Ado.ExecuteCommand(index.Sql);
            }

            // 更新查询优化器统计信息。MySQL 的 ANALYZE 语法与
            // SQLite/PostgreSQL 不同，不能使用双引号表名。
            var analyzeSql = isMySql
                ? $"ANALYZE TABLE {Q("dy_collect_video")}"
                : $"ANALYZE {Q("dy_collect_video")}";
            db.Ado.ExecuteCommand(analyzeSql);

            Log.Information(
                "dy_collect_video 索引检查完成，索引数量={IndexCount}",
                indexes.Length);
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


    // 第一步：定义空Sink（核心，接收日志但不处理）
    public class NullSink : ILogEventSink
    {
        // 空实现：接收到日志事件后直接丢弃
        public void Emit(LogEvent logEvent)
        {
            // 什么都不做，日志直接被丢弃
        }
    }

    // 第二步：扩展方法（方便调用）
    public static class NullSinkExtensions
    {
        public static LoggerConfiguration NullSink(this LoggerSinkConfiguration sinkConfiguration)
        {
            return sinkConfiguration.Sink(new NullSink());
        }
    }

}
