using Microsoft.Extensions.DependencyInjection;
using PlayHouse.Production;
using PlayHouse.Production.Api;
using PlayHouse.Utils;
using Simple;
using SimpleApi.Filter;
using SimpleProtocol;

namespace SimpleApi.handler
{
    [SimpleAspectify]
    public class SampleApiController : IApiController, IApiCallBack
    {
        private LOG<SampleApiController> _log = new();

        public  void Handles(IHandlerRegister register, IBackendHandlerRegister backendRegister)
        {
            register.Add(AuthenticateReq.Descriptor.Index, Authenticate);
            register.Add(HelloReq.Descriptor.Index, Hello);
            register.Add(Simple.CloseSessionMsg.Descriptor.Index, CloseSessionMsg);
            register.Add(Simple.TestTimeoutReq.Descriptor.Index, TestTimeoutReq);
            register.Add(SendMsg.Descriptor.Index,SendMessage);
        }

     
        private async Task TestTimeoutReq(IPacket packet, IApiSender apiSender)
        {
            _log.Debug(()=> $"TestTimeoutReq - accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
            await Task.CompletedTask;
        }

        private async Task Authenticate(IPacket packet, IApiSender apiSender)
        {
            var req = packet.Parse<AuthenticateReq>();
            
            _log.Debug(() => $"Authenticate - [accountId:{req.PlatformUid},sessionEndpoint:{req.Token},sid:{apiSender.Sid}]");

            //string accountId = string.Newstring();
            string accountId = req.PlatformUid;


            apiSender.Authenticate(accountId);

            var message = new Simple.AuthenticateRes {AccountId = accountId, UserInfo = accountId.ToString() };
            apiSender.Reply(new SimplePacket (message));
            await Task.CompletedTask;
        }

        private async Task Hello(IPacket packet, IApiSender apiSender)
        {

            var req = packet.Parse<Simple.HelloReq>();
            _log.Debug(()=>$"Hello - [{req.Message},accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}]");

            //apiSender.Reply(new ReplyPacket (new Simple.HelloRes { Message = "hello" }));
            apiSender.Reply(new SimplePacket(new HelloRes()));
            await Task.CompletedTask;
        }

        private async Task SendMessage(IPacket packet, IApiSender apiSender)
        {
            var recv = packet.Parse<Simple.SendMsg>();
            _log.Debug(() =>$"Message - [message:{recv.Message},accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}]");

            await Task.Delay(100);
            SendTestApiSenderContext(recv.Message);
            await Task.CompletedTask;
        }

        private void SendTestApiSenderContext(string message)
        {
            
            AsyncContext.ApiSender!.SendToClient(new SimplePacket(new Simple.SendMsg() { Message = message}));
        }

        private async Task CloseSessionMsg(IPacket packet, IApiSender apiSender)
        {
            _log.Debug(()=>$"CloseSessionMsg - accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
            apiSender.SendToClient(new SimplePacket(new Simple.CloseSessionMsg()));
            apiSender.SessionClose(apiSender.SessionEndpoint, apiSender.Sid);
            await Task.CompletedTask;
        }


        public async Task OnDisconnect(IApiSender apiSender)
        {
            _log.Debug(()=>$"OnDisconnect - [accountId:{apiSender.AccountId}]");
            await Task.CompletedTask;

        }
    }
}
