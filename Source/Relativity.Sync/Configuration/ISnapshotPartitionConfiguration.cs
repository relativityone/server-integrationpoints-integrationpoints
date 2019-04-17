using System;

namespace Relativity.Sync.Configuration
{
	internal interface ISnapshotPartitionConfiguration : IConfiguration
	{
		int TotalRecordsCount { get; }

		Guid ExportRunId { get; }

		int SourceWorkspaceArtifactId { get; }

		int SyncConfigurationArtifactId { get; }
	}
}