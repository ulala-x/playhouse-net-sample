using PlayHouse.Production;
using PlayHouse.Production.Api.Aspectify;
using PlayHouse.Utils;
using SimpleProtocol;

namespace SimpleApi.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SimpleAspectifyAttribute : AspectifyAttribute
    {
        LOG<SimpleAspectifyAttribute> _log = new();

        
        public override async Task Intercept(Invocation invocation)
        {
            IPacket packet = (IPacket)invocation.Arguments[0];
            
            IApiCommonSender sender = (IApiCommonSender)invocation.Arguments[1];

            if(sender is IApiSender)
            {
                AsyncContext.ApiSender = sender as IApiSender;
            }

            AsyncContext.InitErrorCode();


            SimplePacket simplePacket = (SimplePacket)packet;

            _log.Debug(() => $"from client - [accountId:{sender.AccountId}, transfered:{simplePacket}]");

            await invocation.Proceed();
            foreach (SendPacketInfo packetInfo in PacketContext.SendPackets)
            {
                SendTarget type = packetInfo.Target;

                if(type == SendTarget.ErrorReply)
                {
                    _log.Debug(() => $"send to {packetInfo.Target} - [accountId:{sender.AccountId},errorCode:{packetInfo.ErrorCode}]");
                    
                }
                else if(type == SendTarget.Reply)
                {
                    _log.Debug(() => $"send to {packetInfo.Target} - [accountId:{sender.AccountId},msgSeq:{packetInfo.MsgSeq},transfered:{packet}]");
                }
                else
                {
                    _log.Debug(() => $"send to {packetInfo.Target} - [accountId:{sender.AccountId},transfered:{packet}]");
                }

                
            }

            AsyncContext.Clear();
        }
    }

}
