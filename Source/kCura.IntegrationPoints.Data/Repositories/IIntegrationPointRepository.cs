using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IIntegrationPointRepository
	{
		Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID);
		Task<string> GetFieldMappingJsonAsync(int integrationPointArtifactID);
		string GetSecuredConfiguration(int integrationPointArtifactID);
		string GetName(int integrationPointArtifactID);
	}
}