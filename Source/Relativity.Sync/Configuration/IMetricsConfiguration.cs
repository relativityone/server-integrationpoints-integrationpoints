namespace Relativity.Sync.Configuration
{
	internal interface IMetricsConfiguration : IConfiguration
	{
		string CorrelationId { get; }
		
		string ExecutingApplication { get; }
		
		string ExecutingApplicationVersion { get; }
	}
}