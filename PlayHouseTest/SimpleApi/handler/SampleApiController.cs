using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlayHouse.Production;
using PlayHouse.Production.Api;
using PlayHouse;

namespace SimpleApi.handler
{
    public class SampleApiController : IApiController
    {
        private readonly ILogger<SampleApiController> _log;
 
        public SampleApiController(ILogger<SampleApiController> log)
        {
            _log = log;
        }

        public  void Handles(IHandlerRegister register, IBackendHandlerRegister backendRegister)
        {
            register.Add(Simple.AuthenticateReq.Descriptor.Index, Authenticate);
            register.Add(Simple.HelloReq.Descriptor.Index, Hello);
            register.Add(Simple.CloseSessionMsg.Descriptor.Index, CloseSessionMsg);
            register.Add(Simple.TestTimeoutReq.Descriptor.Index, TestTimeoutReq);
        }

     
        private async Task TestTimeoutReq(Packet packet, IApiSender apiSender)
        {
            _log.LogInformation($"TestTimeoutReq:accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
            await Task.CompletedTask;
        }

        private async Task Authenticate(Packet packet, IApiSender apiSender)
        {
            var req = Simple.AuthenticateReq.Parser.ParseFrom(packet.Data);
            
            _log.LogInformation($"authenticate: platformUid:{req.PlatformUid},token:{req.Token},sid:{apiSender.Sid}");

            Guid accountId = Guid.NewGuid();

            apiSender.Authenticate(accountId);

            var message = new Simple.AuthenticateRes { UserInfo = accountId.ToString() };
            apiSender.Reply(new ReplyPacket (message));
            await Task.CompletedTask;
        }

        private async Task Hello(Packet packet, IApiSender apiSender)
        {
            var req = Simple.HelloReq.Parser.ParseFrom(packet.Data);
            _log.LogInformation($"hello:{req.Message},accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
            apiSender.Reply(new ReplyPacket (new Simple.HelloRes { Message = "hello" }));
            await Task.CompletedTask;
        }

        private async Task SendMessage(Packet packet, IApiSender apiSender)
        {
            var recv = Simple.SendMsg.Parser.ParseFrom(packet.Data);
            _log.LogInformation($"message:{recv.Message},accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");

            await Task.Delay(100);
            SendTestApiSenderContext(recv.Message);
            //apiSender.SendToClient(new Packet(new SendMsg() { Message = recv.Message}));
            await Task.CompletedTask;
        }

        private void SendTestApiSenderContext(string message)
        {
            
            AsyncContext.ApiSender!.SendToClient(new Packet(new Simple.SendMsg() { Message = message}));
        }

        private async Task CloseSessionMsg(Packet packet, IApiSender apiSender)
        {
            _log.LogInformation($"closeSessionMsg - accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
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
