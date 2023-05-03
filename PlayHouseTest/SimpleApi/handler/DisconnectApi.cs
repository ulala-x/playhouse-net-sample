using PlayHouse.Production.Api;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApi.handler
{
    public class DisconnectApi : IApiCallBack
    {
        private readonly ILogger _logger = Log.Logger;

        public void OnDisconnect(long accountId)
        {
            _logger.Information($"OnDisconnect {accountId}");
        }
    }
}
