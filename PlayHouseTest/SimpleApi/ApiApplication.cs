using PlayHouse.Communicator;
using PlayHouse.Service.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SimpleApi.handler;
using PlayHouse.Production;
using PlayHouse.Production.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SimpleApi
{
    public class ApiApplication 
    {
        private readonly Serilog.ILogger _log = Log.Logger;
        private IServiceCollection _services { get; } = new ServiceCollection();

        public void RegisterService()
        {
            //_services.AddLogging
            //var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger,dispose:true);
            _services.AddLogging(builder => builder.AddSerilog(Log.Logger, dispose: true));
            //_services.AddSingleton<ILoggerFactory>(loggerFactory);
            _services.AddTransient<SampleApiController>();
            _services.AddTransient<SampleApiForRoom>();

            GlobalServiceProvider.Instance = _services.BuildServiceProvider();
        }

        public void Run()
        {
            try
            {
                _log.Debug("api start");
                var commonOption = new CommonOption
                {
                    ServerSystem = (systemPanel, baseSender) => new ApiSystem(systemPanel, baseSender),
                    Port = 30470,
                    ServiceId = 2,
                    RedisPort = 6379,
                    RequestTimeoutSec = 0,
                };
                var apiOption = new ApiOption
                {
                    ApiCallBackHandler = new DisconnectApi()
                };
                var apiServer = new ApiServer(commonOption, apiOption);
                apiServer.Start();

                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                {
                    Log.Logger.Information("*** shutting down Api server since process is shutting down");
                    apiServer.Stop();
                    Log.Logger.Information("*** server shut down");
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
