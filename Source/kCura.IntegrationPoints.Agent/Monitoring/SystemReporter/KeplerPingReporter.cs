using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Environmental;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public class KeplerPingReporter : IKeplerPingReporter
    {
        private const string PING_RESPONSE = "OK";
        private readonly IHelper _helper;
        private readonly IAPILog _logger;

        public KeplerPingReporter(IHelper helper, IAPILog logger)
        {
            _helper = helper;
            _logger = logger;
        }

        public bool IsKeplerServiceAccessible()
        {
            return PingKeplerService().GetAwaiter().GetResult();
        }

        private async Task<bool> PingKeplerService()
        {
            bool ping = false;
            try
            {
                using (var pingService = _helper.GetServicesManager().CreateProxy<IPingService>(ExecutionIdentity.System))
                {
                    string pingResponse = await pingService.Ping().ConfigureAwait(false);
                    ping = pingResponse.Equals(PING_RESPONSE);
                    return ping;
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning($"Cannot check Kepler Service Status. Exception {exception}");
            }

            return ping;
        }
    }
}
