using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.HostingBridge.V1.AgentStatusManager;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Services
{
    public class AgentLauncher : IAgentLauncher
    {
        private readonly IServicesMgr _serviceManager;
        private readonly IAPILog _logger;
        private readonly IToggleProvider _toggleProvider;

        public AgentLauncher(IServicesMgr serviceManager, IAPILog logger, IToggleProvider toggleProvider)
        {
            _serviceManager = serviceManager;
            _logger = logger;
            _toggleProvider = toggleProvider;
        }

        public async Task LaunchAgentAsync()
        {
            try
            {
                if (_toggleProvider.IsEnabled<EnableAgentLaunchOnJobStartToggle>())
                {
                    using (IAgentStatusManagerService agentService = _serviceManager.CreateProxy<IAgentStatusManagerService>(ExecutionIdentity.System))
                    {
                        await agentService.StartAgentAsync(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)).ConfigureAwait(false);
                        _logger.LogInformation("StartAgentAsync was called for agent {guid}", Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "StartAgentAsync kepler call failed");
            }
        }
    }
}
