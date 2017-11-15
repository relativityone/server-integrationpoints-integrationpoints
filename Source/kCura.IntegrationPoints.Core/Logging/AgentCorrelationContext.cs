namespace kCura.IntegrationPoints.Core.Logging
{
	public class AgentCorrelationContext : BaseCorrelationContext
	{
		public long JobId { get; set; }
		public long? RootJobId { get; set; }
	}
}
