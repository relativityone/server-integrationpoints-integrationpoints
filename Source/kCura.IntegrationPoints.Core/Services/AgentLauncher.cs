using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.HostingBridge.V1.AgentStatusManager;

namespace kCura.IntegrationPoints.Core.Services
{
    public class AgentLauncher : IAgentLauncher
    {
        private readonly IServicesMgr _serviceManager;
        private readonly IRipToggleProvider _toggleProvider;
        private readonly IAPILog _logger;
        private Guid _agentGuid = Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

        public AgentLauncher(IServicesMgr serviceManager, IRipToggleProvider toggleProvider, IAPILog logger)
        {
            _serviceManager = serviceManager;
            _toggleProvider = toggleProvider;
            _logger = logger;
        }

        public async Task LaunchAgentAsync()
        {
            try
            {
                if (_toggleProvider.IsEnabled<TriggerAgentLaunchOnJobRunToggle>())
                {
                    using (IAgentStatusManagerService agentService = _serviceManager.CreateProxy<IAgentStatusManagerService>(ExecutionIdentity.System))
                    {
                        await agentService.StartAgentAsync(_agentGuid).ConfigureAwait(false);
                        _logger.LogInformation("StartAgentAsync was called for agent {guid}", _agentGuid);
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
