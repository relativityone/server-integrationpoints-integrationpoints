using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Common.Interfaces
{
    public interface IRelativitySyncAppIntegration
    {
        Task SubmitSyncJobAsync(int workspaceArtifactId, int integrationPointArtifactId, int jobHistoryId, int userId);
    }
}
