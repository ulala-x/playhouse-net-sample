using PlayHouse.Production;
using PlayHouse.Production.Session;
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
            const int redisPort = 16379;

            ushort sessionSvcId = 0;
            short apiSvcId = 1;

            var commonOption = new CommonOption
            {
                Port = 10370,
                ServiceId = sessionSvcId,
                RedisPort = redisPort,
                ServerSystem = (systemPanel, sender) => new SessionSystem(systemPanel, sender),
                RequestTimeoutSec = 0,
            };

            var sessionOption = new SessionOption
            {
                SessionPort = 10114,
                ClientSessionIdleTimeout = 0,
                UseWebSocket = false,
                Urls = new List<string> { $"{apiSvcId}:{AuthenticateReq.Descriptor.Index}" }
            };
            
            
            var sessionServer = new SessionServer(commonOption, sessionOption);
            sessionServer.Start();

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                _log.Information("*** shutting down Session server since process is shutting down");
                sessionServer.Stop();
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