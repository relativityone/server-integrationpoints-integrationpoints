using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Environmental;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter
{
    public class KeplerPingReporter : IHealthStatisticReporter, IIsServiceHealthy
    {
        private const string PING_RESPONSE = "OK";
        private readonly IHelper _helper;
        private readonly IAPILog _logger;

        public KeplerPingReporter(IHelper helper, IAPILog logger)
        {
            _helper = helper;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> GetStatisticAsync()
        {
            return new Dictionary<string, object>
            {
                { "IsKeplerServiceAccessible", await IsServiceHealthyAsync().ConfigureAwait(false) }
            };
        }

        public async Task<bool> IsServiceHealthyAsync()
        {
            bool ping = false;
            try
            {
                using (var pingService = _helper.GetServicesManager().CreateProxy<IPingService>(ExecutionIdentity.System))
                {
                    string pingResponse = await pingService.Ping().ConfigureAwait(false);
                    ping = pingResponse.Equals(PING_RESPONSE, StringComparison.InvariantCultureIgnoreCase);
                    return ping;
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, $"Cannot check Kepler Service Status.");
            }

            return ping;
        }
    }
}
