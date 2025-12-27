using ClockSnowFlake;
using dy.net.dto;
using dy.net.model;
using dy.net.utils;
using SqlSugar;
using System.Reflection;
using System.Threading.Tasks;
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
                    UperSaveTogether = false,//博主视频：true-->每个视频单独一个文件夹 false-->所有视频放在同一个文件夹
                    UperUseViedoTitle = false,//博主视频：true-->使用视频标题作为文件名 false-->使用视频id作为文件名
                    DownImageVideo = true,//默认下载图文视频
                    DownMp3 = false,
                    DownImage = false,
                    ImageViedoSaveAlone = true,
                    FollowedTitleTemplate = "",
                    FullFollowedTitleTemplate = "",
                    FollowedTitleSeparator = "",
                    AutoDistinct = true,//默认开启
                    PriorityLevel = "[{\"id\":1,\"name\":\"喜欢的视频\",\"sort\":1},{\"id\":2,\"name\":\"收藏的视频\",\"sort\":2},{\"id\":3,\"name\":\"关注的视频\",\"sort\":3}]",
                    IsFirstRunning = true,
                    OnlySyncNew = true
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
                int update = await sqlSugarClient.Updateable(config).ExecuteCommandAsync();
                return update > 0;
            }

        }

        /// <summary>
        /// 初始化一些数据，兼容旧版数据结构。
        /// </summary>
        public void UpdateCollectViedoType()
        {
            //更新视频类型字段-兼容老版本
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

            //更新关注表的IsNoFollowed字段为空的数据为0--兼容老版本-新加的字段
            string followUpdateSql = @"Update  dy_follow SET IsNoFollowed=0 WHERE IsNoFollowed is NULL";
            sqlSugarClient.Ado.ExecuteCommand(followUpdateSql);

            //更新图片视频的合并状态
            string updateIsMergeVideoSql = @"UPDATE dy_collect_video SET IsMergeVideo = 1 WHERE IsMergeVideo IS NULL and ViedoType='4';";
            sqlSugarClient.Ado.ExecuteCommand(updateIsMergeVideoSql);

            //更新非图片视频的合并状态
            string updateNoIsMergeVideoSql = @"UPDATE dy_collect_video SET IsMergeVideo = 0 WHERE IsMergeVideo IS NULL and ViedoType<>'4';";
            sqlSugarClient.Ado.ExecuteCommand(updateNoIsMergeVideoSql);

            //强制开启去重
            sqlSugarClient.Updateable<AppConfig>().SetColumns(x => new AppConfig { AutoDistinct = true }).Where(x => !string.IsNullOrWhiteSpace(x.Id)).ExecuteCommand();
            //重新根据保存路径更新图片视频的类型为喜欢，收藏或关注
            var collectViedos = sqlSugarClient.Queryable<DouyinVideo>().Where(x=>x.ViedoType==VideoTypeEnum.ImageVideo).ToList();
            if (collectViedos != null && collectViedos.Any()) 
            {
                var cookies= sqlSugarClient.Queryable<DouyinCookie>().ToList();
                collectViedos.ForEach(x =>
                {
                 var ck= cookies.FirstOrDefault(c => c.Id == x.CookieId);
                    if (ck != null)
                    {

                        var savePath = x.VideoSavePath;
                        if (savePath != null)
                        {
                            if (savePath.StartsWith(ck.SavePath))
                            {
                                x.ViedoType = VideoTypeEnum.dy_collects;
                            }
                            else if(savePath.StartsWith(ck.UpSavePath))
                            {
                                x.ViedoType = VideoTypeEnum.dy_follows;
                            }
                            else if(savePath.StartsWith(ck.FavSavePath))
                            {
                                x.ViedoType = VideoTypeEnum.dy_favorite;
                            }
                            else
                            {
                                x.ViedoType= VideoTypeEnum.dy_collects;
                            }
                        }
                    }
                });
                //只更新图片视频的类型
                sqlSugarClient.Updateable(collectViedos).UpdateColumns(x => new DouyinVideo { ViedoType = x.ViedoType }).IgnoreColumns(true).ExecuteCommand();
            }
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
        /// 查询是否已删除
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        public async Task<bool> ExistDeleteVideo(string videoId)
        {
            var count= await sqlSugarClient.Queryable<DouyinVideoDelete>().Where(x=>x.ViedoId==videoId).CountAsync();
            return count > 0;
        }

        /// <summary>
        /// 新增要删除的视频
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> AddDeleteVideo(DouyinVideoDelete dto)
        {
            dto.Id=IdGener.GetLong().ToString();
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
