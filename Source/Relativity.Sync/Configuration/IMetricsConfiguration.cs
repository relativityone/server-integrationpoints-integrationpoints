namespace Relativity.Sync.Configuration
{
	internal interface IMetricsConfiguration : IConfiguration
	{
		string CorrelationId { get; }
		
		string ExecutingApplication { get; }
		
		string ExecutingApplicationVersion { get; }

		string DataSourceType { get; }

		string DataDestinationType { get; }

		bool ImageImport { get; }

		int? JobHistoryToRetryId { get; }

		string SyncVersion { get; }
	}
}