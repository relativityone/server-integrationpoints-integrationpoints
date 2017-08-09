namespace kCura.IntegrationPoints.Data
{
	public class IntegrationPointQuery : IntegrationPointBaseQuery<IntegrationPoint>, IIntegrationPointQuery
	{
		public IntegrationPointQuery(IRSAPIService context) : base(context)
		{
		}
	}
}