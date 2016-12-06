using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface IAllDestinationProvidersWithNameQueryBuilder
	{
		Query<RDO> Create();
	}
}