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
            return sqlSugarClient.Queryable<AppConfig>().First();
        }
        /// <summary>
        /// 初始化并返回配置
        /// </summary>
        /// <param name="downLoadImage"></param>
        /// <returns></returns>
        public AppConfig InitConfig()
        {
            var conf = GetConfig();
            if (conf != null)
            {
                return conf;
            }
            else
            {
                AppConfig config = new AppConfig
                {
                    Id = IdGener.GetLong().ToString(),
                    Cron = 30,
                    BatchCount = 18,
                    LogKeepDay = 10,
                    UperSaveTogether = false,//博主视频：true-->每个视频单独一个文件夹 false-->所有视频放在同一个文件夹
                    UperUseViedoTitle = false,//博主视频：true-->使用视频标题作为文件名 false-->使用视频id作为文件名
                    DownImageVideo = false,//默认不下载图文视频
                    DownMp3 = false,
                    DownImage = false,
                    ImageViedoSaveAlone = true,
                    FollowedTitleTemplate = "",
                    FullFollowedTitleTemplate = "",
                    FollowedTitleSeparator = "",
                    AutoDistinct = true//默认开启
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
                cc.DownImageVideo = config.DownImageVideo;
                cc.UperSaveTogether = config.UperSaveTogether;
                cc.UperUseViedoTitle = config.UperUseViedoTitle;
                cc.LogKeepDay = config.LogKeepDay;
                cc.DownMp3 = config.DownMp3;
                cc.DownImage = config.DownImage;
                cc.FollowedTitleTemplate = config.FollowedTitleTemplate;
                cc.FullFollowedTitleTemplate= config.FullFollowedTitleTemplate;
                cc.ImageViedoSaveAlone = config.ImageViedoSaveAlone;
                cc.FollowedTitleSeparator = config.FollowedTitleSeparator;
                cc.AutoDistinct = config.AutoDistinct;
            }

            int update = await sqlSugarClient.Updateable(cc).ExecuteCommandAsync();

            return update > 0;
        }

        /// <summary>
        /// 兼容旧版将之前同步了的我收藏的数据进行更新
        /// </summary>
        public void UpdateCollectViedoType()
        {

            string sql = @"UPDATE dy_collect_video 
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

            var cookies = sqlSugarClient.Queryable<DouyinCookie>().ToList();
            foreach (var cookie in cookies)
            {
                cookie.CollHasSyncd = 0;
                cookie.FavHasSyncd = 0;
                cookie.UperSyncd = 0;
            }
            return sqlSugarClient.Updateable(cookies).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 查询需要下载的所有重下载视频记录
        /// </summary>
        /// <returns></returns>
        public async Task<List<ViedoReDown>> GetAllRedown()
        {
            return await sqlSugarClient.Queryable<ViedoReDown>().Where(x => x.Status == 0 || x.Status == 2).ToListAsync();
        }

        public async Task<bool> UpdateRedownStatus(List<ViedoReDown> list)
        {
            return await sqlSugarClient.Updateable(list).ExecuteCommandAsync() > 0;
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
