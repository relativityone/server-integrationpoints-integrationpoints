namespace Relativity.Sync.Configuration
{
	interface IJobEndMetricsConfiguration : ISumReporterConfiguration
	{
		int SyncConfigurationArtifactId { get; }

		ImportOverwriteMode ImportOverwriteMode { get; }

		DataSourceType DataSourceType { get; }

		DestinationLocationType DestinationType { get; }

		ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }

		ImportImageFileCopyMode ImportImageFileCopyMode { get; }
	}
}