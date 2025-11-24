using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
using dy.net.utils;
using SqlSugar;
using System.Reflection;
using System.Threading.Tasks;

namespace dy.net.service
{
    public class DouyinCommonService
    {
        private readonly ISqlSugarClient sqlSugarClient;

        public DouyinCommonService(ISqlSugarClient sqlSugarClient)
        {
            this.sqlSugarClient = sqlSugarClient;
        }

        public AppConfig GetConfig()
        {
            var config = sqlSugarClient.Queryable<AppConfig>().First();

            var downImgConfig = Appsettings.Get(SystemStaticUtil.DOWN_IMAGE_VIDEO_ENABLE);
            if (config != null && !string.IsNullOrEmpty(downImgConfig))
            {
                downImgConfig = downImgConfig.ToLower();
                config.DownImageVideoFromEnv = downImgConfig == "1";
            }
            return config;
        }
        /// <summary>
        /// 初始化并返回配置
        /// </summary>
        /// <param name="downLoadImage"></param>
        /// <returns></returns>
        public AppConfig InitConfig(bool downLoadImage)
        {
            var conf = GetConfig();
            if (conf != null)
            {
                conf.DownImageVideo = downLoadImage;
                sqlSugarClient.Updateable(conf).ExecuteCommand();
                return conf;
            }
            else
            {
                AppConfig config = new AppConfig
                {
                    Id = IdGener.GetLong().ToString(),
                    Cron = "30",
                    BatchCount = 10,
                    DownImageVideo = downLoadImage,
                    LogKeepDay = 10
                };
                sqlSugarClient.Insertable(config).ExecuteCommand();
                return config;
            }


        }


        public async Task<bool> UpdateConfig(AppConfig config)
        {

            var cc = await sqlSugarClient.Queryable<AppConfig>().FirstAsync(x => x.Id == config.Id);

            if (cc == null)
                return false;

            else
            {
                cc.Cron = config.Cron;
                cc.BatchCount = config.BatchCount;
            }

            var update = await sqlSugarClient.Updateable<AppConfig>(cc).ExecuteCommandAsync();

            return update > 0;
        }

        /// <summary>
        /// 兼容旧版将之前同步了的我收藏的数据进行更新
        /// </summary>
        public void UpdateCollectViedoType()
        {

            string sql= @"UPDATE dy_collect_video 
                        SET ViedoType = CASE 
                        WHEN ViedoType = '0' THEN 0 
                        WHEN ViedoType = '1' THEN 1  
                        WHEN ViedoType = '2' THEN 2 
                        WHEN ViedoType = '3' THEN 3
                        WHEN ViedoType = '4' THEN 4  
                        ELSE NULL 
                    END;";

            sqlSugarClient.Ado.ExecuteCommand(sql);
            //var collectViedos = sqlSugarClient.Queryable<DouyinVideo>().ToList();

            //if (collectViedos.Any())
            //{
            //    collectViedos.ForEach(x =>
            //    {
            //        x.ViedoType = x.;
            //    });
            //    sqlSugarClient.Updateable(collectViedos).ExecuteCommand();
            //}
        }
        /// <summary>
        /// 重置所有Cookie的同步状态为0
        /// </summary>
        /// <returns></returns>
        public bool UpdateAllCookieSyncedToZero()
        {
            //var sql = "update dy_cookie set CollHasSyncd=0,FavHasSyncd=0,UperSyncd=0";
            // sqlSugarClient.Ado.ExecuteCommand(sql) > 0;
            var cookies = sqlSugarClient.Queryable<DouyinUserCookie>().ToList();

            foreach (var cookie in cookies)
            {
                cookie.CollHasSyncd = 0;
                cookie.FavHasSyncd = 0;
                cookie.UperSyncd = 0;
                var upers = cookie.UpSecUserIds;
                if (!string.IsNullOrWhiteSpace(upers))
                {
                    var uperList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DouyinUpSecUserIdDto>>(upers);
                    if (uperList != null && uperList.Count > 0)
                    {
                        foreach (var uper in uperList)
                        {
                            uper.syncAll = false;
                        }
                    }
                    var newUpers = Newtonsoft.Json.JsonConvert.SerializeObject(uperList);
                    cookie.UpSecUserIds = newUpers;
                }
            }
            return sqlSugarClient.Updateable(cookies).ExecuteCommand() > 0;
        }

        #region 测试创建数据库

        ///// <summary>
        ///// 创建SQLite数据库连接字符串
        ///// </summary>
        //private  string CreateSqliteDBConn()
        //{
        //    var dbFolder = Path.Combine(Environment.CurrentDirectory, "db");
        //    Directory.CreateDirectory(dbFolder); // 不存在则创建，无需判断

        //    var dbPath = Path.Combine(dbFolder, "dy.sqlite");
        //    if (!File.Exists(dbPath))
        //    {
        //        using (File.Create(dbPath)) { } // 使用using确保文件流关闭
        //    }

        //    return $"DataSource={dbPath}";
        //}
        ///// <summary>
        ///// 初始化数据库
        ///// </summary>
        //public  ISqlSugarClient InitDataBase(DbType dbType, string connString)
        //{
        //    // 处理SQLite连接字符串
        //    if (dbType == DbType.Sqlite)
        //    {
        //        connString = CreateSqliteDBConn();
        //    }

        //    if (string.IsNullOrWhiteSpace(connString))
        //    {
        //        return null;
        //    }

        //    return new SqlSugarClient(new ConnectionConfig
        //    {
        //        ConnectionString = connString,
        //        InitKeyType = InitKeyType.Attribute,
        //        DbType = dbType,
        //        IsAutoCloseConnection = true
        //    }, db =>
        //    {
        //        // 日志配置
        //        db.Aop.OnLogExecuting = (sql, pars) => Serilog.Log.Debug(sql);
        //        db.Aop.OnError = e =>
        //        {
        //            Serilog.Log.Error(e.Message);
        //            Serilog.Log.Error(e.Sql);
        //        };

        //        // 创建数据库和表
        //        db.DbMaintenance.CreateDatabase();
        //        var modelTypes = Assembly.GetExecutingAssembly()
        //                                 .GetTypes()
        //                                 .Where(t => t.Namespace?.StartsWith("dy.net.model") ?? false)
        //                                 .ToArray();
        //        db.CodeFirst.InitTables(modelTypes);
        //    });
        //}
        #endregion

    }

}
