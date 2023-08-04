using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices
{
    public interface IEntityFullNameObjectManagerService
    {
        Task<int> GetFullNameArtifactId(int workspaceId);

        Task<bool> IsEntityAsync(int workspaceId, int artifactTypeId);
    }
}
