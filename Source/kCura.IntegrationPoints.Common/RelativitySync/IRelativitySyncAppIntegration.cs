using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Common.RelativitySync
{
    public interface IRelativitySyncAppIntegration
    {
        Task SubmitSyncJobAsync(int workspaceArtifactId, int integrationPointArtifactId, int jobHistoryId, int userId);
        Task CancelJobAsync(Guid jobId);
    }
}
