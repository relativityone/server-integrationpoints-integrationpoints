using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface IIntegrationPointByProvidersQueryBuilder
	{
		Query<RDO> CreateQuery(int sourceProviderArtifactId, int destinationProviderArtifactId);
	}
}