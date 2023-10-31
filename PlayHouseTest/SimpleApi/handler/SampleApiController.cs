using Microsoft.Extensions.DependencyInjection;
using PlayHouse.Communicator.Message;
using PlayHouse.Production;
using PlayHouse.Production.Api;
using Serilog;

namespace SimpleApi.handler
{
    public class SampleApiController : IApiController
    {
        private readonly ILogger _log = Log.Logger;


        public  void Handles(IHandlerRegister register, IBackendHandlerRegister backendRegister)
        {
            register.Add(Simple.AuthenticateReq.Descriptor.Index, Authenticate);
            register.Add(Simple.HelloReq.Descriptor.Index, Hello);
            register.Add(Simple.CloseSessionMsg.Descriptor.Index, CloseSessionMsg);
            register.Add(Simple.TestTimeoutReq.Descriptor.Index, TestTimeoutReq);
        }

     
        private async Task TestTimeoutReq(Packet packet, IApiSender apiSender)
        {
            _log.Debug("TestTimeoutReq:accountId:{0},sessionEndpoint:{1},sid:{2}",apiSender.AccountId,apiSender.SessionEndpoint,apiSender.Sid);
            await Task.CompletedTask;
        }

        private async Task Authenticate(Packet packet, IApiSender apiSender)
        {
            var req = Simple.AuthenticateReq.Parser.ParseFrom(packet.Data);
            
            _log.Debug("Authenticate: platformUid:{0},token:{1},sid:{2}",req.PlatformUid,req.Token,apiSender.Sid);

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
            _log.Debug("Hello:{0},accountId:{1},sessionEndpoint:{2},sid:{3}",req.Message,apiSender.AccountId,apiSender.SessionEndpoint,apiSender.Sid);

            //apiSender.Reply(new ReplyPacket (new Simple.HelloRes { Message = "hello" }));
            apiSender.Reply(new ReplyPacket(0,Simple.HelloRes.Descriptor.Index));
            await Task.CompletedTask;
        }

        private async Task SendMessage(Packet packet, IApiSender apiSender)
        {
            var recv = Simple.SendMsg.Parser.ParseFrom(packet.Data);
            _log.Debug(
                "Message:{0},accountId:{1},sessionEndpoint:{2},sid:{3}",
                recv.Message,apiSender.AccountId,apiSender.SessionEndpoint,apiSender.Sid
            );

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
            _log.Debug(
                "CloseSessionMsg - accountId:{0},sessionEndpoint:{1},sid:{2}",
                apiSender.AccountId,apiSender.SessionEndpoint,apiSender.Sid
            );
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
