using System;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync
{
    public interface IJobHistorySyncService
    {
        Task<RelativityObject> GetLastJobHistoryWithErrorsAsync(int workspaceId, int integrationPointArtifactId);

        Task<DateTime?> GetLastCompletedJobHistoryForRunDateAsync(int workspaceId, int integrationPointArtifactId);

        Task UpdateFinishedJobAsync(IExtendedJob job, ChoiceRef status, bool hasErrors);

        Task AddJobHistoryErrorAsync(IExtendedJob job, Exception e);
    }
}
