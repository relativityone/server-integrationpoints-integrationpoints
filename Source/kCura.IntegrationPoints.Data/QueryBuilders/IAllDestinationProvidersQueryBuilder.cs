using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface IAllDestinationProvidersQueryBuilder
	{
		Query<RDO> Create();
	}
}