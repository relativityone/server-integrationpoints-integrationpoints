using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface IDestinationProviderArtifactIdByGuidQueryBuilder
	{
		Query<RDO> Create(string guid);
	}
}