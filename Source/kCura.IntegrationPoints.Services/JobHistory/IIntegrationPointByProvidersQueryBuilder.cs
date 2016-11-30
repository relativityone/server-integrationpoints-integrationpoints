using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IIntegrationPointByProvidersQueryBuilder
	{
		Query<RDO> CreateQuery(int sourceProviderArtifactId, int destinationProviderArtifactId);
	}
}