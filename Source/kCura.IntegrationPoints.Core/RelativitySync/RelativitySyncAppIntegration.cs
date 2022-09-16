using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Interfaces;
using kCura.IntegrationPoints.Common.RelativitySync;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.RelativitySync
{
    public class RelativitySyncAppIntegration : IRelativitySyncAppIntegration
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly IIntegrationPointToSyncAppConverter _integrationPointToSyncAppConverter;
        private readonly IAPILog _logger;

        public RelativitySyncAppIntegration(IServicesMgr servicesMgr, IIntegrationPointToSyncAppConverter integrationPointToSyncAppConverter, IAPILog logger)
        {
            _servicesMgr = servicesMgr;
            _integrationPointToSyncAppConverter = integrationPointToSyncAppConverter;
            _logger = logger;
        }

        public async Task SubmitSyncJobAsync(int workspaceArtifactId, int integrationPointArtifactId, int jobHistoryId, int userId)
        {
            try
            {
                await _integrationPointToSyncAppConverter.CreateSyncConfigurationAsync(workspaceArtifactId, integrationPointArtifactId, jobHistoryId, userId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit Sync job");
                throw;
            }
        }
    }
}
