using Microsoft.Extensions.DependencyInjection;
using PlayHouse.Production.Session;
using PlayHouse.Production.Shared;
using PlayHouse.Service.Session;
using Serilog;
using Simple;
using SimpleProtocol;

namespace SimpleSession;

internal class SessionApplication
{
    private readonly ILogger _log = Log.Logger;

    public void Run()
    {
        try
        {

            var services = new ServiceCollection();

            services.AddScoped<ISystemController, SimpleSessionSystem>();

            var commonOption = new PlayhouseOption
            {
                Ip = "127.0.0.1",
                Port = 10370,
                ServiceId = (int)ServiceId.Session,
                ServerId = 0,
                PacketProducer = (msgId, payload, msgSeq) => new SimplePacket(msgId, payload, msgSeq),
                ServiceProvider = services.BuildServiceProvider()
            };

            var sessionOption = new SessionOption
            {
                SessionPort = 10114,
                UseWebSocket = false,
                Urls = [$"{(int)ServiceId.Api}:{AuthenticateReq.Descriptor.Name}"],
                ClientIdleTimeoutMSec = 0,
                SessionUserFactory = (sender)=> new SimpleUser(sender)

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