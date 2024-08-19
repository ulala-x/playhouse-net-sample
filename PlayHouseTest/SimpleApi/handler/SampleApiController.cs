using PlayHouse.Production.Api;
using PlayHouse.Production.Shared;
using PlayHouse.Service.Shared;
using PlayHouse.Utils;
using Simple;
using SimpleApi.Filter;
using SimpleProtocol;

namespace SimpleApi.handler;

[SimpleAspectify]
public class SampleApiController : IApiController, IDisconnectCallback
{
    private readonly LOG<SampleApiController> _log = new();

    public void Handles(IHandlerRegister register)
    {
        register.Add(AuthenticateReq.Descriptor.Name, Authenticate);
        register.Add(HelloReq.Descriptor.Name, Hello);
        register.Add(Simple.CloseSessionMsg.Descriptor.Name, CloseSessionMsg);
        register.Add(Simple.TestTimeoutReq.Descriptor.Name, TestTimeoutReq);
        register.Add(SendMsg.Descriptor.Name, SendMessage);
        register.Add(Action_PlayActionReq.Descriptor.Name,ReqPlayerActionReq);

        for (int i = 0; i < 1000; i++)
        {
            register.Add($"{HelloReq.Descriptor.Name}_{i}", HelloX);

        }
        
    }

    private async Task HelloX(IPacket packet, IApiSender apisender)
    {
        var data = DataProto.Parser.ParseFrom(packet.Payload.DataSpan);
        //_log.Info(()=> $"{packet.MsgId}");
        //_log.Info(() => $"{data.Message}");


        apisender.Reply(new SimplePacket(new DataProto()
        {
            Message = data.Message
        }));
        await Task.CompletedTask;
    }

    private async Task ReqPlayerActionReq(IPacket packet, IApiSender apiSender)
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


    private async Task TestTimeoutReq(IPacket packet, IApiSender apiSender)
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


        apiSender.Authenticate(accountId);

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
            Message = "Hello"
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

    private async Task CloseSessionMsg(IPacket packet, IApiSender apiSender)
    {
        _log.Debug(() =>
            $"CloseSessionMsg - accountId:{apiSender.AccountId},sessionEndpoint:{apiSender.SessionEndpoint},sid:{apiSender.Sid}");
        apiSender.SendToClient(new SimplePacket(new CloseSessionMsg()));
        apiSender.SessionClose(apiSender.SessionEndpoint, apiSender.Sid);
        await Task.CompletedTask;
    }
}