namespace kCura.IntegrationPoints.Data
{
	public class IntegrationPointQuery : IntegrationPointBaseQuery<IntegrationPoint>
	{
		public IntegrationPointQuery(IRSAPIService context) : base(context)
		{
		}
	}
}