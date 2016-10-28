using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IIntegrationPointRepository
	{
		Task<IntegrationPointModel> CreateIntegrationPointAsync(CreateIntegrationPointRequest request);
		Task<IntegrationPointModel> UpdateIntegrationPointAsync(UpdateIntegrationPointRequest request);
		Task<IntegrationPointModel> GetIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId);
		Task RunIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId);
		Task<IList<IntegrationPointModel>> GetAllIntegrationPointsAsync(int workspaceArtifactId);
		Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier);
		Task<int> GetIntegrationPointArtifactTypeIdAsync(int workspaceArtifactId);
	}
}