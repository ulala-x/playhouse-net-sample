using PlayHouse.Communicator.Message;
using PlayHouse.Production;
using PlayHouse.Service;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSession
{
    class SessionSystem : IServerSystem
    {
        ISystemPanel _panel;
        ISender _sender;
        private readonly ILogger _log = Log.Logger;

        public SessionSystem(ISystemPanel panel, ISender sender)
        {
            _panel = panel;
            _sender = sender;
        }

        public async Task OnDispatch(Packet packet)
        {
            _log.Information("OnDispatch");
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
