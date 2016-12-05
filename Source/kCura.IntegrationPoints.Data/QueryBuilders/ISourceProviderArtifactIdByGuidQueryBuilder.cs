using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface ISourceProviderArtifactIdByGuidQueryBuilder
	{
		Query<RDO> Create(string guid);
	}
}