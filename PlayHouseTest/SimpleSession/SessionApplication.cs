using Microsoft.Extensions.DependencyInjection;
using PlayHouse.Production.Session;
using PlayHouse.Production.Shared;
using PlayHouse.Service.Session;
using Serilog;
using Simple;

namespace SimpleSession;

class SessionApplication
{
    private readonly ILogger _log = Log.Logger;

    public void Run()
    {
        try
        {
            ushort sessionSvcId = 0;
            ushort apiSvcId = 1;

            ServiceCollection services = new ServiceCollection();

            var commonOption = new CommonOption
            {
                Port = 10370,
                ServiceId = sessionSvcId,
                RequestTimeoutSec = 0,
                NodeId = 0,
                AddressServerServiceId = apiSvcId,
                AddressServerEndpoints = { "10.12.20.59:10470" },
                ServiceProvider = services.BuildServiceProvider(),
            };

            var sessionOption = new SessionOption
            {
                SessionPort = 10114,
                UseWebSocket = false,
                Urls = new List<string> { $"{apiSvcId}:{AuthenticateReq.Descriptor.Index}" },
                ClientIdleTimeoutMSec = 5000
            };
            
            
            var sessionServer = new SessionServer(commonOption, sessionOption);
            sessionServer.Start();

            AppDomain.CurrentDomain.ProcessExit += async (sender, e) =>
            {
                _log.Information("*** shutting down Session server since process is shutting down");
                await sessionServer.StopAsync();
                _log.Information("*** server shut down");
                Thread.Sleep(1000);
            };

            _log.Information("Session Server Started");
            sessionServer.AwaitTermination();
        }
        catch (Exception e)
        {
            _log.Error(e.StackTrace!);
            Environment.Exit(1);
        }
    }
}