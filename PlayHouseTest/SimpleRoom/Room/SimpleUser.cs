using PlayHouse.Production.Play;
using PlayHouse.Service.Play;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlay.Room
{
    internal class SimpleUser : IActor
    {
        private ILogger _log = Log.Logger;
        private IActorSender _sender;
        public IActorSender ActorSender => _sender;

        public SimpleUser(IActorSender sender) { 
            _sender = sender;
        }

        public async Task OnCreate()
        {
            _log.Debug($"OnCreate - [accountId:{_sender.AccountId()}]");
            await Task.CompletedTask;
        }

        public async Task OnDestroy()
        {
            _log.Debug($"OnDestroy - [accountId:{_sender.AccountId()}]");
            await Task.CompletedTask;
        }

        internal long AccountId => ActorSender.AccountId();
    }
}
