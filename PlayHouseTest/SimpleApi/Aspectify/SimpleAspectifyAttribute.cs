using PlayHouse.Production.Api.Aspectify;
using PlayHouse.Production.Shared;
using PlayHouse.Utils;
using SimpleProtocol;

namespace SimpleApi.Filter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SimpleAspectifyAttribute : AspectifyAttribute
{
    private readonly LOG<SimpleAspectifyAttribute> _log = new();


    public override async Task Intercept(Invocation invocation)
    {
        var packet = (IPacket)invocation.Arguments[0];

        var sender = (IApiCommonSender)invocation.Arguments[1];

        if (sender is IApiSender)
        {
            AsyncContext.ApiSender = sender as IApiSender;
        }

        AsyncContext.InitErrorCode();


        var simplePacket = (SimplePacket)packet;

        _log.Debug(() => $"from client - [accountId:{sender.AccountId}, transfered:{simplePacket}]");

        await invocation.Proceed();
        
        foreach (var packetInfo in PacketContext.SendPackets)
        {
            var type = packetInfo.Target;

            if (type == SendTarget.ErrorReply)
            {
                _log.Debug(() =>
                    $"send to {packetInfo.Target} - [accountId:{sender.AccountId},errorCode:{packetInfo.ErrorCode}]");
            }
            else if (type == SendTarget.Reply)
            {
                _log.Debug(() =>
                    $"send to {packetInfo.Target} - [accountId:{sender.AccountId},msgSeq:{packetInfo.MsgSeq},transfered:{packet}]");
            }
            else
            {
                _log.Debug(() => $"send to {packetInfo.Target} - [accountId:{sender.AccountId},transfered:{packet}]");
            }
        }

        AsyncContext.Clear();
    }
}