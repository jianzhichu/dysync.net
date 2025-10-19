using dy.net.dto;
using dy.net.model;
using SqlSugar;
using System.Threading.Tasks;

namespace dy.net.service
{
    public class CommonService
    {
        private readonly ISqlSugarClient sqlSugarClient;

        public CommonService(ISqlSugarClient sqlSugarClient)
        {
            this.sqlSugarClient = sqlSugarClient;
        }

        public  AppConfig GetConfig()
        {
            return  sqlSugarClient.Queryable<AppConfig>().First();
        }

        public AppConfig InitConfig(AppConfig config)
        {
            var conf = GetConfig();
            if (conf != null)
                return conf;
            else
            {
                var add = sqlSugarClient.Insertable<AppConfig>(config).ExecuteCommand();
                return config;
            }
        }


        public async Task<bool> UpdateConfig(AppConfig config) {

            var cc = await sqlSugarClient.Queryable<AppConfig>().FirstAsync(x=>x.Id== config.Id);

            if (cc == null)
                return false;

            else {
                cc.Cron = config.Cron;
                cc.BatchCount = config.BatchCount;
            }

            var update = await sqlSugarClient.Updateable<AppConfig>(cc).ExecuteCommandAsync();

            return update > 0 ;
        }

        /// <summary>
        /// 兼容旧版将之前同步了的我收藏的数据进行更新
        /// </summary>
        public void UpdateCollectViedoType()
        {
            var collectViedos= sqlSugarClient.Queryable<DyCollectVideo>().Where(x=>string.IsNullOrWhiteSpace(x.ViedoType)).ToList();

            if (collectViedos.Any())
            {
                collectViedos.ForEach(x => {
                    x.ViedoType = "2";
                });
                sqlSugarClient.Updateable(collectViedos).ExecuteCommand();
            }
        }
        /// <summary>
        /// 重置所有Cookie的同步状态为0
        /// </summary>
        /// <returns></returns>
        public bool UpdateAllCookieSyncedToZero()
        {
            //var sql = "update dy_cookie set CollHasSyncd=0,FavHasSyncd=0,UperSyncd=0";
            // sqlSugarClient.Ado.ExecuteCommand(sql) > 0;
            var cookies= sqlSugarClient.Queryable<DyUserCookies>().ToList();

            foreach (var cookie in cookies)
            {
                cookie.CollHasSyncd = 0;
                cookie.FavHasSyncd = 0;
                cookie.UperSyncd = 0;
                var upers = cookie.UpSecUserIds;
                if (!string.IsNullOrWhiteSpace(upers))
                {
                    var uperList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DyUpSecUserIdDto>>(upers);
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


    }
}
