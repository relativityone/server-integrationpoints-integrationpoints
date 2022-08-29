using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Environmental;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public class KeplerPingReporter : IKeplerPingReporter
    {
        private const string PING_RESPONSE = "OK";
        private readonly IHelper _helper;

        public KeplerPingReporter(IHelper helper)
        {
            _helper = helper;
        }

        public bool IsKeplerServiceAccessible()
        {
            return PingKeplerService().GetAwaiter().GetResult();
        }

        private async Task<bool> PingKeplerService()
        {
            using (var pingService = _helper.GetServicesManager().CreateProxy<IPingService>(ExecutionIdentity.System))
            {
                string pingResponse = await pingService.Ping().ConfigureAwait(false);
                return pingResponse.Equals(PING_RESPONSE);
            }
        }
    }
}