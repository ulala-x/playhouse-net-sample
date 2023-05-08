using PlayHouse.Production;
using PlayHouse.Production.Play;
using PlayHouse.Service.Api;
using PlayHouse.Service.Play;
using Serilog;
using SimplePlay.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlay
{
    internal class PlayApplication
    {
        private ILogger _log = Log.Logger;
        internal void Run()
        {
            try
            {
                var redisPort = 6379;
                var commonOption = new CommonOption()
                {
                    Port = 30570,
                    ServiceId = 3,
                    RedisPort = redisPort,
                    ServerSystem = (systemPanel, sender) => new PlaySystem(systemPanel, sender),
                    RequestTimeoutSec = 0,
                };

                var playOption = new PlayOption();

                playOption.PlayProducer.Register(
                    "simple", 
                    (stageSender) => new SimpleRoom(stageSender), 
                    (actorSender) => new SimpleUser(actorSender)
                );

                var playServer = new PlayServer(commonOption, playOption);
                playServer.Start();

                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                {
                    _log.Information("*** shutting down Play server since process is shutting down");
                    playServer.Stop();
                    _log.Information("*** server shut down");
                    Thread.Sleep(1000);
                };

                _log.Information("Play Server Started");
                playServer.AwaitTermination();

            }
            catch(Exception ex)
            {
                _log.Error(ex, ex.StackTrace ?? ex.Message);
            }
        }
    }
}
