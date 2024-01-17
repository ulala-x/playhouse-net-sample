using PlayHouse.Service.Api;
using Serilog;
using SimpleApi.handler;
using PlayHouse.Production;
using PlayHouse.Production.Api;
using Microsoft.Extensions.DependencyInjection;
using ILogger = Serilog.ILogger;
using SimpleProtocol;
using PlayHouse.Communicator.Message;
using PlayHouse.Production.Shared;
using SimpleApi.System;

namespace SimpleApi
{
    public class ApiApplication 
    {
        private readonly ILogger _log = Log.Logger;

        public void Run()
        {
            try
            {

                ushort apiSvcId = 1;
                ServiceCollection services = new ServiceCollection();
                services.AddScoped<SampleApiController>();
                services.AddScoped<SampleApiForRoom>();
                services.AddScoped<SimpleApiSystem>();

                
                _log.Information("api start");
                var commonOption = new PlayhouseOption
                {
                    Port = 10470,
                    ServiceId = apiSvcId,
                    RequestTimeoutSec = 0,
                    NodeId = 1,
                    PacketProducer = (int msgId,IPayload paylaod,ushort msgSeq) => new SimplePacket(msgId,paylaod, msgSeq),
                    AddressServerServiceId = apiSvcId,
                    AddressServerEndpoints = { "10.12.20.59:10470" },
                    ServiceProvider = services.BuildServiceProvider(),
                };

                //AddressServerEndpoints = { "tcp://127.0.0.1:10470" },
                var apiServer = new ApiServer(commonOption, new ApiOption());
                apiServer.Start();

                AppDomain.CurrentDomain.ProcessExit +=  async (sender, eventArgs) =>
                {
                    _log.Information("*** shutting down Api server since process is shutting down");
                    await apiServer.StopAsync();
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
