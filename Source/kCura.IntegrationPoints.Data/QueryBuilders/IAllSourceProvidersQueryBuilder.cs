using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface IAllSourceProvidersQueryBuilder
	{
		Query<RDO> Create();
	}
}