using System.Collections.Concurrent;
using Google.Protobuf;
using PlayHouse.Production.Api;
using PlayHouse.Production.Shared;
using PlayHouse.Utils;
using Simple;
using SimpleApi.Filter;
using SimpleProtocol;

namespace SimpleApi.handler;

[SimpleAspectify]
public class SampleApiController : IApiController, IDisconnectCallback
{
    private readonly LOG<SampleApiController> _log = new();
    private static ConcurrentDictionary<ushort,IMessage> _messages = new();

    public void Handles(IHandlerRegister register)
    {
        register.Add(AuthenticateReq.Descriptor.Name, Authenticate);
        register.Add(HelloReq.Descriptor.Name, Hello);
        register.Add(CloseSessionMsg.Descriptor.Name, MsgCloseSession);
        register.Add(TestTimeoutReq.Descriptor.Name, ReqTestTimeout);
        register.Add(SendMsg.Descriptor.Name, SendMessage);
        register.Add(Action_PlayActionReq.Descriptor.Name,ReqPlayerAction);
        register.Add(CompressReq.Descriptor.Name,ReqCompress);
        register.Add(DuplicatedPacketReq.Descriptor.Name,ReqDuplicatedPacket);

        for (int i = 0; i < 1000; i++)
        {
            register.Add($"{HelloReq.Descriptor.Name}_{i}", HelloX);

        }
        
    }

    private async Task ReqDuplicatedPacket(IPacket packet, IApiSender sender)
    {
        SimplePacket simplePacket = (SimplePacket)packet;

        var duplicateReq = DuplicatedPacketReq.Parser.ParseFrom(packet.Payload.DataSpan);
        
        if (_messages.TryGetValue(simplePacket.MsgSeq, out var message))
        {
            sender.Reply(new SimplePacket(message));
        }
        else
        {
            var duplicatedPacketRes = new DuplicatedPacketRes
            {
                Data = duplicateReq.Data
            };  
            _messages[simplePacket.MsgSeq] = duplicatedPacketRes;
            sender.Reply(new SimplePacket(duplicatedPacketRes));
        }

        await Task.CompletedTask;

    }

    private async Task ReqCompress(IPacket packet, IApiSender sender)
    {
        sender.Reply(new SimplePacket(new CompressRes
        {
            Data = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        }));
        await Task.CompletedTask;
    }

    private async Task HelloX(IPacket packet, IApiSender sender)
    {
        var data = DataProto.Parser.ParseFrom(packet.Payload.DataSpan);
        //_log.Info(()=> $"{packet.MsgId}");
        //_log.Info(() => $"{data.Message}");


        sender.Reply(new SimplePacket(new DataProto()
        {
            Message = data.Message
        }));
        await Task.CompletedTask;
    }

    private async Task ReqPlayerAction(IPacket packet, IApiSender apiSender)
    {
        var req = packet.Parse<Action_PlayActionReq>();
        var res = new Action_PlayActionRes
        {
            Type = req.Type,
            Value1 = req.Value1,
            Value2 = req.Value2,
            Value3 = req.Value3
        };

        apiSender.Reply(new SimplePacket(res));
        await Task.CompletedTask;
    }


    public async Task OnDisconnectAsync(IApiSender apiSender)
    {
        _log.Debug(() => $"OnDisconnect - [accountId:{apiSender.AccountId}]");
        await Task.CompletedTask;
    }


    private async Task ReqTestTimeout(IPacket packet, IApiSender apiSender)
    {
        _log.Debug(() =>
            $"TestTimeoutReq - accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
        await Task.CompletedTask;
    }

    private async Task Authenticate(IPacket packet, IApiSender apiSender)
    {
        var req = packet.Parse<AuthenticateReq>();

        _log.Debug(
            () => $"Authenticate - [accountId:{req.PlatformUid},sessionEndpoint:{req.Token},sid:{apiSender.Sid}]");

        //string accountId = string.Newstring();
        var accountId = long.Parse(req.PlatformUid);
        //long accountId = ControlContext.SystemPanel.GenerateUUID();


        await apiSender.AuthenticateAsync(accountId);

        var message = new AuthenticateRes { AccountId = accountId, UserInfo = accountId.ToString() };
        apiSender.Reply(new SimplePacket(message));
        await Task.CompletedTask;
    }

    private async Task Hello(IPacket packet, IApiSender apiSender)
    {
        var req = packet.Parse<HelloReq>();
        _log.Debug(() =>
            $"Hello - [{req.Message},accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}]");

        //apiSender.Reply(new ReplyPacket (new Simple.HelloRes { Message = "hello" }));
        apiSender.Reply(new SimplePacket(new HelloRes()
        {
            Message = req.Message
        }));
        await Task.CompletedTask;
    }

    private async Task SendMessage(IPacket packet, IApiSender apiSender)
    {
        var recv = packet.Parse<SendMsg>();
        _log.Debug(() =>
            $"Message - [message:{recv.Message},accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}]");

        await Task.Delay(100);
        SendTestApiSenderContext(recv.Message);
        await Task.CompletedTask;
    }

    private void SendTestApiSenderContext(string message)
    {
        AsyncContext.ApiSender!.SendToClient(new SimplePacket(new SendMsg { Message = message }));
    }

    private async Task MsgCloseSession(IPacket packet, IApiSender apiSender)
    {
        _log.Debug(() =>
            $"CloseSessionMsg - accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
        apiSender.SendToClient(new SimplePacket(new CloseSessionMsg()));
        apiSender.SessionClose(apiSender.SessionEndpoint, apiSender.Sid);
        await Task.CompletedTask;
    }
}