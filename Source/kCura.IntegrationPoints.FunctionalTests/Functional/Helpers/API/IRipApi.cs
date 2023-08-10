using System.Threading.Tasks;
using Relativity.IntegrationPoints.Services;
using WorkloadDiscovery;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.API
{
    public interface IRipApi
    {
        Task CreateIntegrationPointAsync(IntegrationPointModel integrationPoint, int workspaceId);
        Task<int> RunIntegrationPointAsync(IntegrationPointModel integrationPoint, int workspaceId);
        Task<int> RetryIntegrationPointAsync(IntegrationPointModel integrationPoint, int workspaceId, bool switchToAppendOverlayMode = false);
        Task<string> GetJobHistoryStatus(int jobHistoryId, int workspaceId);
        Task WaitForJobToFinishAsync(int jobHistoryId, int workspaceId, int checkDelayInMs = 500, string expectedStatus = "Completed");
    }
}
