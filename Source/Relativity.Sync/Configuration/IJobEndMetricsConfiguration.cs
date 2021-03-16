namespace Relativity.Sync.Configuration
{
	interface IJobEndMetricsConfiguration : IConfiguration
	{
		int? JobHistoryToRetryId { get; }

		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }

		int SyncConfigurationArtifactId { get; }

		ImportOverwriteMode ImportOverwriteMode { get; }

		DataSourceType DataSourceType { get; }

		DestinationLocationType DestinationType { get; }

		ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }

		ImportImageFileCopyMode ImportImageFileCopyMode { get; }
	}
}