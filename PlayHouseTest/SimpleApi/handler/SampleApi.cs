using Google.Protobuf;
using Playhouse.Simple.Protocol;
using PlayHouse.Production;
using PlayHouse.Production.Api;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace SimpleApi.handler
{
    public class SampleApi : IApiService
    {
        private ISystemPanel? _systemPanel;
        private ISender? _sender;
        private ILogger _log = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        public  async Task Init(ISystemPanel systemPanel, ISender sender)
        {
            this._systemPanel = systemPanel;
            this._sender = sender;
            await Task.CompletedTask;
        }

        public  IApiService Instance()
        {
            return new SampleApi();
        }

        public  void Handles(IHandlerRegister register, IBackendHandlerRegister backendRegister)
        {
            register.Add(AuthenticateReq.Descriptor.Index, Authenticate);
            register.Add(HelloReq.Descriptor.Index, Hello);
            register.Add(Playhouse.Simple.Protocol.CloseSessionMsg.Descriptor.Index, CloseSessionMsg);
            register.Add(SendMsg.Descriptor.Index, SendMessage);
        }

     

        private async Task Authenticate(Packet packet, IApiSender apiSender)
        {
            var req = AuthenticateReq.Parser.ParseFrom(packet.Data);
            long accountId = req.UserId;
            _log.Information($"authenticate: accountId:{accountId},token:{req.Token},sid:{apiSender.Sid}");

            apiSender.Authenticate(accountId);

            var message = new AuthenticateRes { UserInfo = accountId.ToString() };
            apiSender.Reply(new ReplyPacket (message));
            await Task.CompletedTask;
        }

        private async Task Hello(Packet packet, IApiSender apiSender)
        {
            var req = HelloReq.Parser.ParseFrom(packet.Data);
            _log.Information($"hello:{req.Message},accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
            apiSender.Reply(new ReplyPacket (new HelloRes { Message = "hello" }));
            await Task.CompletedTask;
        }

        private async Task SendMessage(Packet packet, IApiSender apiSender)
        {
            var recv = SendMsg.Parser.ParseFrom(packet.Data);
            _log.Information($"message:{recv.Message},accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
            apiSender.SendToClient(new Packet(new SendMsg() { Message = recv.Message}));
            await Task.CompletedTask;
        }

        private async Task CloseSessionMsg(Packet packet, IApiSender apiSender)
        {
            _log.Information($"closeSessionMsg - accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
            apiSender.SendToClient(new Packet (new CloseSessionMsg()));
            apiSender.SessionClose(apiSender.SessionEndpoint, apiSender.Sid);
            await Task.CompletedTask;
        }
    }
}
