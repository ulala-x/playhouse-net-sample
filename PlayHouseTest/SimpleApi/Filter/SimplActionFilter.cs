using PlayHouse.Production;
using PlayHouse.Production.Api.Filter;
using PlayHouse.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApi.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SimplActionFilter : ApiActionFilterAttribute
    {
        LOG<SimplActionFilter> _log = new();
        public override void AfterExecution(IPacket packet, IApiSender apiSender)
        {
            
        }

        public override void BeforeExecution(IPacket packet, IApiSender apiSender)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SimpleBackendActionFilter : ApiBackendActionFilterAttribute
    {
        public override void AfterExecution(IPacket packet, IApiBackendSender apiSender)
        {
        }

        public override void BeforeExecution(IPacket packet, IApiBackendSender apiSender)
        {
        }
    }
}
