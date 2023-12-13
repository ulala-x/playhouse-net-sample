using PlayHouse.Service.Api;
using Serilog;
using SimpleApi.handler;
using PlayHouse.Production;
using PlayHouse.Production.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using SimpleConfigure;

namespace SimpleApi
{
    public class ApiApplication 
    {
        private readonly ILogger _log = Log.Logger;
        private IServiceCollection Services { get; } = new ServiceCollection();

        public void RegisterService()
        {
            // Services.AddLogging(
            //     builder =>
            //     {
            //         builder.AddSerilog(Log.Logger, dispose: true);
            //         //builder.SetMinimumLevel(LogLevel.Information);
            //     });
            Services.AddTransient<SampleApiController>();
            Services.AddTransient<SampleApiForRoom>();

            GlobalServiceProvider.Instance = Services.BuildServiceProvider();
        }

        public void Run()
        {
            try
            {
                
                
                _log.Information("api start");
                var commonOption = new CommonOption
                {
                    ServerSystem = (systemPanel, baseSender) => new ApiSystem(systemPanel, baseSender),
                    Port = 10470,
                    ServiceId = 1,
                    RedisPort = 16379,
                    RequestTimeoutSec = 0,
                    NodeId = 1,
                };
                var apiOption = new ApiOption
                {
                    ApiCallBackHandler = new DisconnectApi()
                };
                var apiServer = new ApiServer(commonOption, apiOption);
                apiServer.Start();

                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                {
                    _log.Information("*** shutting down Api server since process is shutting down");
                    apiServer.Stop();
                    _log.Information("*** server shut down");
                    Thread.Sleep(1000);
                };

                _log.Information("Api Server Started");
                apiServer.AwaitTermination();
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.StackTrace ?? ex.Message);
            }
        }
    }
}
