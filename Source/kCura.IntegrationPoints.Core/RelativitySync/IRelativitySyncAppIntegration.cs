using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.RelativitySync
{
    public interface IRelativitySyncAppIntegration
    {
        Task SubmitSyncJobAsync(int workspaceArtifactId, IntegrationPointDto integrationPointDto, int jobHistoryId, int userId);
        Task CancelJobAsync(Guid jobId);
    }
}
