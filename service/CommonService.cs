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
    }
}
