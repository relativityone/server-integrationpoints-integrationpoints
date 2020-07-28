namespace Relativity.Sync.Configuration
{
	interface IJobEndMetricsConfiguration : ISumReporterConfiguration
	{
		int SyncConfigurationArtifactId { get; }
	}
}