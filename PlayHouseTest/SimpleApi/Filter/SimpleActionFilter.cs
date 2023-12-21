using PlayHouse.Production;
using PlayHouse.Production.Api.Filter;
using PlayHouse.Utils;
using Serilog;
using SimpleProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApi.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SimpleActionFilter : ApiActionFilterAttribute
    {
        LOG<SimpleActionFilter> _log = new();


        public override void BeforeExecution(IPacket packet, IApiSender apiSender)
        {
            var simplePacket = (SimplePacket)packet;
            _log.Debug(() => $"from client - [accountId:{apiSender.AccountId}, transfered:{simplePacket}]");
        }
        public override void AfterExecution(IPacket packet, IApiSender apiSender)
        {

            foreach (var transfered in AsyncContext.SendPackets)
            {
                _log.Debug(() => $"send to {transfered.target} - [accountId:{apiSender.AccountId},transfered:{transfered.packet}]");
            }
        }

    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SimpleBackendActionFilter : ApiBackendActionFilterAttribute
    {
        LOG<SimpleBackendActionFilter> _log = new();

        public override void BeforeExecution(IPacket packet, IApiBackendSender apiSender)
        {
            var simplePacket = (SimplePacket)packet;
            _log.Debug(() => $"from client - [accountId:{apiSender.AccountId}, transfered:{simplePacket}]");
        }
        public override void AfterExecution(IPacket packet, IApiBackendSender apiSender)
        {
            foreach (var transfered in AsyncContext.SendPackets)
            {
                _log.Debug(() => $"send to {transfered.target} - [accountId:{apiSender.AccountId},transfered:{transfered.packet}]");
            }
        }

        
    }
}
