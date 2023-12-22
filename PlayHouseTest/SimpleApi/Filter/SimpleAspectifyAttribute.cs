using PlayHouse.Production;
using PlayHouse.Production.Api.Aspectify;
using PlayHouse.Production.Api.Filter;
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
            SimplePacket simplePacket = (SimplePacket)packet;
            IApiCommonSender sender = (IApiCommonSender)invocation.Arguments[1];

            _log.Debug(() => $"from client - [accountId:{sender.AccountId}, transfered:{simplePacket}]");

            await invocation.Proceed();
            foreach (var transfered in AsyncContext.SendPackets)
            {
                _log.Debug(() => $"send to {transfered.target} - [accountId:{sender.AccountId},transfered:{transfered.packet}]");
            }
        }
    }

}
