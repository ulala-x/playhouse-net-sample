using PlayHouse.Production;
using PlayHouse.Service;
using Serilog;
using Simple;
using SimpleProtocol;

namespace SimpleApi
{
    public class ApiSystem : IServerSystem
    {
        private readonly ISender _sender;
        private readonly ISystemPanel _panel;
        private ILogger _log = Log.Logger;

        public ApiSystem(ISystemPanel systemPanel, ISender sender)
        {
            _panel = systemPanel;
            _sender = sender;

            GlobalControlProvider.SystemPanel = systemPanel;
            GlobalControlProvider.Sender = sender;
        }

        public async Task OnDispatch(IPacket packet)
        {
            _log.Information($"{packet.MsgId} packet received");

            try
            {
                if (packet.MsgId == HelloReq.Descriptor.Index)
                {
                    var message = packet.Parse<HelloReq>().Message;
                    _sender.Reply(new SimplePacket(new HelloRes { Message = message }));
                    await Task.CompletedTask;
                    
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, ex.StackTrace ?? ex.Message);
            }
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
