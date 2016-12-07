using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface IAllIntegrationPointTypesQueryBuilder
	{
		Query<RDO> Create();
	}
}