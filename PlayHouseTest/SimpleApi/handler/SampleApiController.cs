using Microsoft.Extensions.DependencyInjection;
using PlayHouse.Production;
using PlayHouse.Production.Api;
using PlayHouse.Utils;

namespace SimpleApi.handler
{
    public class SampleApiController : IApiController
    {
        private LOG<SampleApiController> _log = new();

        public  void Handles(IHandlerRegister register, IBackendHandlerRegister backendRegister)
        {
            register.Add(Simple.AuthenticateReq.Descriptor.Index, Authenticate);
            register.Add(Simple.HelloReq.Descriptor.Index, Hello);
            register.Add(Simple.CloseSessionMsg.Descriptor.Index, CloseSessionMsg);
            register.Add(Simple.TestTimeoutReq.Descriptor.Index, TestTimeoutReq);
            register.Add(Simple.SendMsg.Descriptor.Index,SendMessage);
        }

     
        private async Task TestTimeoutReq(Packet packet, IApiSender apiSender)
        {
            _log.Debug(()=> $"TestTimeoutReq - accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
            await Task.CompletedTask;
        }

        private async Task Authenticate(Packet packet, IApiSender apiSender)
        {
            var req = Simple.AuthenticateReq.Parser.ParseFrom(packet.Data);
            
            _log.Debug(() => $"TestTimeoutReq - [accountId:{req.PlatformUid},sessionEndpoint:{req.Token},sid:{apiSender.Sid}]");

            //Guid accountId = Guid.NewGuid();
            Guid accountId = Guid.Parse(req.PlatformUid);

            apiSender.Authenticate(accountId);

            var message = new Simple.AuthenticateRes { UserInfo = accountId.ToString() };
            apiSender.Reply(new ReplyPacket (message));
            await Task.CompletedTask;
        }

        private async Task Hello(Packet packet, IApiSender apiSender)
        {

            var req = Simple.HelloReq.Parser.ParseFrom(packet.Data);
            _log.Debug(()=>$"Hello - [{req.Message},accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}]");

            //apiSender.Reply(new ReplyPacket (new Simple.HelloRes { Message = "hello" }));
            apiSender.Reply(new ReplyPacket(0,Simple.HelloRes.Descriptor.Index));
            await Task.CompletedTask;
        }

        private async Task SendMessage(Packet packet, IApiSender apiSender)
        {
            var recv = Simple.SendMsg.Parser.ParseFrom(packet.Data);
            _log.Debug(() =>$"Message - [message:{recv.Message},accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}]");

            await Task.Delay(100);
            SendTestApiSenderContext(recv.Message);
            await Task.CompletedTask;
        }

        private void SendTestApiSenderContext(string message)
        {
            
            AsyncContext.ApiSender!.SendToClient(new Packet(new Simple.SendMsg() { Message = message}));
        }

        private async Task CloseSessionMsg(Packet packet, IApiSender apiSender)
        {
            _log.Debug(()=>$"CloseSessionMsg - accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
            apiSender.SendToClient(new Packet (new Simple.CloseSessionMsg()));
            apiSender.SessionClose(apiSender.SessionEndpoint, apiSender.Sid);
            await Task.CompletedTask;
        }

        public IApiController Instance()
        {
            return GlobalServiceProvider.Instance.GetService<SampleApiController>()! ;
        }
    }
}
