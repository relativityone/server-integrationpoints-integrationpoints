using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface IAllSourceProvidersWithNameQueryBuilder
	{
		Query<RDO> Create();
	}
}