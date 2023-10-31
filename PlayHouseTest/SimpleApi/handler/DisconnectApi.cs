using PlayHouse.Production.Api;
using Serilog;


namespace SimpleApi.handler
{
    public class DisconnectApi : IApiCallBack
    {
        private readonly ILogger _logger = Log.Logger;

        public void OnDisconnect(Guid accountId)
        {
            _logger.Debug("OnDisconnect {0}",accountId);
        }
    }
}
