using System.Threading.Tasks;
using Relativity.IntegrationPoints.Services;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.API
{
    public interface IRipApi
    {
        Task CreateIntegrationPointAsync(IntegrationPointModel integrationPoint, int workspaceId);
        Task<int> RunIntegrationPointAsync(IntegrationPointModel integrationPoint, int workspaceId);
        Task<string> GetJobHistoryStatus(int jobHistoryId, int workspaceId);
    }
}