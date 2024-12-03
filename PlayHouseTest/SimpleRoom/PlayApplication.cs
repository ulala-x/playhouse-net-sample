using Microsoft.Extensions.DependencyInjection;
using PlayHouse.Production.Play;
using PlayHouse.Production.Shared;
using PlayHouse.Service.Play;
using Serilog;
using Simple;
using SimplePlay.Room;
using SimpleProtocol;

namespace SimplePlay;

internal class PlayApplication
{
    private readonly ILogger _log = Log.Logger;

    internal void Run()
    {
        try
        {
            var services = new ServiceCollection();
            services.AddScoped<ISystemController, SimplePlaySystem>();

            var commonOption = new PlayhouseOption
            {
                Ip = "127.0.0.1",
                Port = 10570,
                ServiceId = (int)ServiceId.Play,
                ServerId = 0,
                RequestTimeoutSec = 0,
                PacketProducer = (msgId, payload, msgSeq) => new SimplePacket(msgId, payload, msgSeq),
                ServiceProvider = services.BuildServiceProvider()
            };

            var playOption = new PlayOption();

            playOption.PlayProducer.Register(
                "simple",
                stageSender => new SimpleRoom(stageSender),
                actorSender => new SimpleUser(actorSender)
            );


            var playServer = new PlayServer(commonOption, playOption);
            playServer.Start();

            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) =>
            {
                _log.Information("*** shutting down Play server since process is shutting down");
                await playServer.StopAsync();
                _log.Information("*** server shut down");
                Thread.Sleep(1000);
            };

            _log.Information("Play Server Started");
            playServer.AwaitTermination();
        }
        catch (Exception ex)
        {
            _log.Error(ex, ex.StackTrace ?? ex.Message);
        }
    }
}