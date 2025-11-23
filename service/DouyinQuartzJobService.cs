using Quartz;
using dy.net.job;

namespace dy.net.service
{
    public class DouyinQuartzJobService
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public DouyinQuartzJobService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public async Task StartJob(string expression,int delay=5000)
        {
            await StartCollectJob(expression);

            await Task.Delay(delay);
            //如果是数字则加1分钟，减少并发
            if (int.TryParse(expression, out int cron))
            {
                cron++;
                expression = cron.ToString();
            }
            await StartFavoriteJob(expression);

            await Task.Delay(delay);
            if (int.TryParse(expression, out int cron2))
            {
                cron2++;
                expression = cron2.ToString();
            }
            await StartUperPostJob(expression);
        }

        private async Task<bool> StartCollectJob(string expression)
        {
            try
            {
                var __scheduler1 = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey("dy.job.key.collect", "group1");
                var triggerKey = new TriggerKey("dy.trigger.key.collect", "group1");

                var jobDetail = await __scheduler1.GetJobDetail(jobKey);
                if (jobDetail != null)
                {
                    //await __scheduler1.Shutdown();
                    await __scheduler1.DeleteJob(jobKey);//删掉原来的
                }

                IJobDetail job = JobBuilder.Create<DouyinCollectSyncJob>()
                 .WithIdentity(jobKey)
                 .Build();
                ITrigger trigger;
                //var expression = Appsettings.Get("Interval");
                if (CronExpression.IsValidExpression(expression))
                {
                    trigger = TriggerBuilder.Create()
                     .WithIdentity(triggerKey)
                     .WithCronSchedule(expression)
                     .StartAt(DateTime.Now.AddSeconds(30))
                     .StartNow()
                     .Build();
                }
                else
                {
                    int Interval = int.TryParse(expression, out int _Interval) ? _Interval : 30;
                    trigger = TriggerBuilder.Create()
                   .WithIdentity(triggerKey)
                   //.StartNow()
                   .StartAt(DateTime.Now.AddSeconds(3))
                   .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(Interval)
                    .RepeatForever())
                   .Build();
                }
                // Tell Quartz to schedule the job using our trigger
                await __scheduler1.ScheduleJob(job, trigger);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("start dy.collect job error", ex);
                return false;
            }
            return true;
        }


        private async Task<bool> StartFavoriteJob(string expression)
        {
            try
            {
                var __scheduler1 = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey("dy.job.key.favorite", "group1");
                var triggerKey = new TriggerKey("dy.trigger.key.favorite", "group1");

                var jobDetail = await __scheduler1.GetJobDetail(jobKey);
                if (jobDetail != null)
                {
                    //await __scheduler1.Shutdown();
                    await __scheduler1.DeleteJob(jobKey);//删掉原来的
                }

                IJobDetail job = JobBuilder.Create<DouyinFavoritSyncJob>()
                 .WithIdentity(jobKey)
                 .Build();
                ITrigger trigger;
                //var expression = Appsettings.Get("Interval");
                if (CronExpression.IsValidExpression(expression))
                {
                    trigger = TriggerBuilder.Create()
                     .WithIdentity(triggerKey)
                     .WithCronSchedule(expression)
                     .StartAt(DateTime.Now.AddSeconds(30))
                     .StartNow()
                     .Build();
                }
                else
                {
                    int Interval = int.TryParse(expression, out int _Interval) ? _Interval : 30;
                    trigger = TriggerBuilder.Create()
                   .WithIdentity(triggerKey)
                   //.StartNow()
                   .StartAt(DateTime.Now.AddSeconds(3))
                   .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(Interval)
                    .RepeatForever())
                   .Build();
                }
                // Tell Quartz to schedule the job using our trigger
                await __scheduler1.ScheduleJob(job, trigger);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("start dy.favorite job error", ex);
                return false;
            }
            return true;
        }

        private async Task<bool> StartUperPostJob(string expression)
        {
            try
            {
                var __scheduler1 = await _schedulerFactory.GetScheduler();
                var jobKey = new JobKey("dy.job.key.uper", "group1");
                var triggerKey = new TriggerKey("dy.trigger.key.uper", "group1");

                var jobDetail = await __scheduler1.GetJobDetail(jobKey);
                if (jobDetail != null)
                {
                    //await __scheduler1.Shutdown();
                    await __scheduler1.DeleteJob(jobKey);//删掉原来的
                }

                IJobDetail job = JobBuilder.Create<DouyinUperPostSyncJob>()
                 .WithIdentity(jobKey)
                 .Build();
                ITrigger trigger;
                //var expression = Appsettings.Get("Interval");
                if (CronExpression.IsValidExpression(expression))
                {
                    trigger = TriggerBuilder.Create()
                     .WithIdentity(triggerKey)
                     .WithCronSchedule(expression)
                     .StartAt(DateTime.Now.AddSeconds(30))
                     .StartNow()
                     .Build();
                }
                else
                {
                    int Interval = int.TryParse(expression, out int _Interval) ? _Interval : 30;
                    trigger = TriggerBuilder.Create()
                   .WithIdentity(triggerKey)
                   //.StartNow()
                   .StartAt(DateTime.Now.AddSeconds(3))
                   .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(Interval)
                    .RepeatForever())
                   .Build();
                }
                // Tell Quartz to schedule the job using our trigger
                await __scheduler1.ScheduleJob(job, trigger);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("start dy.uper job error", ex);
                return false;
            }
            return true;
        }

    }
}
