using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.API
{
    public interface IRipApi
    {
        Task CreateIntegrationPoint(IntegrationPointModel integrationPoint, int workspaceId);
        Task<int> RunIntegrationPoint(IntegrationPointModel integrationPoint, int workspaceId);
        Task<string> CheckJobStatus(int jobId);
    }
}