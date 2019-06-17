namespace Relativity.Sync.Configuration
{
	interface IJobEndMetricsConfiguration : ISumReporterConfiguration
	{
		int SourceWorkspaceArtifactId { get; }

		int SyncConfigurationArtifactId { get; }
	}
}