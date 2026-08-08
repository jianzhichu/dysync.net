namespace dy.net.service
{
    public class ApplicationRestartService
    {
        private readonly IHostApplicationLifetime _applicationLifetime;

        public ApplicationRestartService(IHostApplicationLifetime applicationLifetime)
        {
            _applicationLifetime = applicationLifetime;
        }

        public void RestartAfterResponse(TimeSpan? delay = null)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(delay ?? TimeSpan.FromSeconds(1.5));
                Serilog.Log.Information("数据库已切换，停止当前进程并由部署管理器热重启");
                _applicationLifetime.StopApplication();
            });
        }
    }
}
