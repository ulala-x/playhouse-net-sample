using PlayHouse.Production;
using PlayHouse.Service;
using Serilog;
using Simple;

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

        public async Task OnDispatch(Packet packet)
        {
            _log.Information($"{packet.MsgId} packet received");

            try
            {
                if (packet.MsgId == HelloReq.Descriptor.Index)
                {
                    var message = HelloReq.Parser.ParseFrom(packet.Data).Message;
                    _sender.Reply(new ReplyPacket(new HelloRes { Message = message }));
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
