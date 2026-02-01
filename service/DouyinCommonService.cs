using ClockSnowFlake;
using dy.net.model.entity;
using SqlSugar;
//using static Org.BouncyCastle.Math.EC.ECCurve;

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
        /// <returns></returns>
        public AppConfig InitConfig()
        {
            var conf = GetConfig();
            if (conf != null)
            {
                if (string.IsNullOrWhiteSpace(conf.PriorityLevel))
                {
                    conf.PriorityLevel = "[{\"id\":1,\"name\":\"喜欢的视频\",\"sort\":1},{\"id\":2,\"name\":\"收藏的视频\",\"sort\":2},{\"id\":3,\"name\":\"关注的视频\",\"sort\":3}]";
                }
                conf.IsFirstRunning = true;//标记为程序刚启动第一次运行
                conf.AutoDistinct = true;
                if(!conf.VideoEncoder.HasValue)
                conf.VideoEncoder = 264;
                sqlSugarClient.Updateable(conf).ExecuteCommand();
                //兼容旧版本
                return conf;
            }
            else
            {
                AppConfig config = new AppConfig
                {
                    Id = IdGener.GetLong().ToString(),
                    Cron = 30,
                    BatchCount = 18,
                    LogKeepDay = 7,
                    //UperSaveTogether = false,//博主视频：true-->每个视频单独一个文件夹 false-->所有视频放在同一个文件夹
                    //UperUseViedoTitle = false,//博主视频：true-->使用视频标题作为文件名 false-->使用视频id作为文件名
                    DownImageVideo = true,//默认下载图文视频
                    DownMp3 = false,
                    DownImage = false,
                    FollowedTitleTemplate = "",
                    FullFollowedTitleTemplate = "",
                    FollowedTitleSeparator = "",
                    AutoDistinct = true,//默认开启
                    PriorityLevel = "[{\"id\":1,\"name\":\"喜欢的视频\",\"sort\":1},{\"id\":2,\"name\":\"收藏的视频\",\"sort\":2},{\"id\":3,\"name\":\"关注的视频\",\"sort\":3}]",
                    IsFirstRunning = true,
                    OnlySyncNew = true,
                    DownDynamicVideo = false,
                    KeepDynamicVideo = false,
                    MegDynamicVideo = true,
                    VideoEncoder = 264
                };
                sqlSugarClient.Insertable(config).ExecuteCommand();
                return config;
            }


        }

        /// <summary>
        /// 更新AppConfig的IsFirstRunning属性为fasle
        /// </summary>
        /// <returns></returns>
        public async Task SetConfigNotFirstRunning()
        {
            //更新IsFirstRunning为false
            await sqlSugarClient.Updateable<AppConfig>()
                .SetColumns(x => x.IsFirstRunning == false)
                .Where(x => x.IsFirstRunning == true)
                .ExecuteCommandAsync();
        }


        /// <summary>
        /// 更新配置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>

        public async Task<bool> UpdateConfig(AppConfig config)
        {

            var cc = await sqlSugarClient.Queryable<AppConfig>().FirstAsync(x => x.Id == config.Id);
            if (cc == null)
                return false;

            else
            {
                int update = await sqlSugarClient.Updateable(config).IgnoreColumns(x => new { x.IsFirstRunning }).ExecuteCommandAsync();
                return update > 0;
            }

        }

        /// <summary>
        /// 重置所有Cookie的同步状态为0
        /// </summary>
        /// <returns></returns>
        //public bool UpdateAllCookieSyncedToZero()
        //{

        //    var cookies = sqlSugarClient.Queryable<DouyinCookie>().ToList();
        //    foreach (var cookie in cookies)
        //    {
        //        cookie.CollHasSyncd = 0;
        //        cookie.FavHasSyncd = 0;
        //        cookie.UperSyncd = 0;
        //    }
        //    return sqlSugarClient.Updateable(cookies).ExecuteCommand() > 0;
        //}

        /// <summary>
        /// 查询是否已删除
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        public async Task<bool> ExistDeleteVideo(string videoId)
        {
            var count = await sqlSugarClient.Queryable<DouyinVideoDelete>().Where(x => x.ViedoId == videoId).CountAsync();
            return count > 0;
        }

        /// <summary>
        /// 新增要删除的视频
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> AddDeleteVideo(DouyinVideoDelete dto)
        {
            dto.Id = IdGener.GetLong().ToString();
            dto.DeleteTime = DateTime.Now;
            return sqlSugarClient.Insertable(dto).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 查询已删除视频列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<DouyinVideoDelete>> GetDouyinDeleteVideos()
        {
            return await sqlSugarClient.Queryable<DouyinVideoDelete>()
                .ToListAsync();
        }


        ///// <summary>
        ///// 获取已经下载过视频的作者的保存路径
        ///// </summary>
        ///// <param name="uperId"></param>
        ///// <returns></returns>
        //public async Task<string> GetDouyinUpSavePath(string uperId)
        //{
        //    return await sqlSugarClient.Queryable<DouyinVideoUp>().Where(x => x.UperId == uperId).Select(x => x.SavePath).FirstAsync();
        //}
        ///// <summary>
        ///// 保存下载过视频的作者的保存路径
        ///// </summary>
        ///// <param name="uperId"></param>
        ///// <param name="SavePath"></param>
        ///// <returns></returns>
        //public async Task<bool> SaveDouyinUpSavePath(string uperId, string SavePath)
        //{

        //    var upSavePath = await GetDouyinUpSavePath(uperId);
        //    if (string.IsNullOrWhiteSpace(upSavePath))
        //    {
        //       await sqlSugarClient.Insertable(new DouyinVideoUp
        //        {
        //            Id = IdGener.GetLong().ToString(),
        //            SavePath = SavePath,
        //            UperId = uperId
        //        }).ExecuteCommandAsync();
        //    }
        //    return true;

        //}

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
