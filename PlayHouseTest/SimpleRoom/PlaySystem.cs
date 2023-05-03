using PlayHouse.Production;
using PlayHouse.Service;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePlay
{
    internal class PlaySystem : IServerSystem
    {
        private ISystemPanel _panel;
        private ISender _sender;
        private ILogger _log = Log.Logger;
        public PlaySystem(ISystemPanel panel,ISender sender) 
        { 
            _panel = panel;
            _sender = sender;
        }

        public async Task OnDispatch(Packet packet)
        {
            _log.Information($"OnDispatch msgId:{packet.MsgId}");
            await Task.CompletedTask;
        }

        public async Task OnPause()
        {
            _log.Information("OnPause");
            await Task.CompletedTask;
        }

        public async Task OnResume()
        {
            _log.Information("OnResume");
            await Task.CompletedTask;
        }

        public async Task OnStart()
        {
            _log.Information("OnStart");
            await Task.CompletedTask;
        }

        public async Task OnStop()
        {
            _log.Information("OnStop");
            await Task.CompletedTask;
        }
    }
}
