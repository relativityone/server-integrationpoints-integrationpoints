using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.RelativitySync;
using Relativity.API;
using Relativity.Sync.Services.Interfaces.V1;
using Relativity.Sync.Services.Interfaces.V1.DTO;

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
                int syncConfigurationId = await _integrationPointToSyncAppConverter.CreateSyncConfigurationAsync(workspaceArtifactId, integrationPointArtifactId, jobHistoryId, userId).ConfigureAwait(false);
                using (ISyncService syncService = _servicesMgr.CreateProxy<ISyncService>(ExecutionIdentity.System))
                {
                    SubmitJobRequestDTO request = new SubmitJobRequestDTO()
                    {
                        WorkspaceID = workspaceArtifactId,
                        UserID = userId,
                        SyncConfigurationArtifactID = syncConfigurationId
                    };
                    Guid jobId = await syncService.SubmitJobAsync(request).ConfigureAwait(false);
                    _logger.LogInformation("Sync job has been submitted. Job ID: {jobId}", jobId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit Sync job");
                throw;
            }
        }
    }
}
