using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data
{
	public class IntegrationPointQuery : IntegrationPointBaseQuery<IntegrationPoint>, IIntegrationPointQuery
	{
		public IntegrationPointQuery(IRelativityObjectManager relativityObjectManager) : base(relativityObjectManager)
		{
		}
	}
}