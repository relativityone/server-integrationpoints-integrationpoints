using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.HostingBridge.V1.AgentStatusManager;

namespace kCura.IntegrationPoints.Core.Services
{
    public class AgentLauncher : IAgentLauncher
    {
        private readonly IServicesMgr _serviceManager;
        private readonly IAPILog _logger;

        public AgentLauncher(IServicesMgr serviceManager, IAPILog logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
        }

        public async Task LaunchAgentAsync()
        {
            try
            {
                using (IAgentStatusManagerService agentService = _serviceManager.CreateProxy<IAgentStatusManagerService>(ExecutionIdentity.System))
                {
                    await agentService.StartAgentAsync(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "StartAgentAsync kepler call failed");
            }
        }
    }
}
