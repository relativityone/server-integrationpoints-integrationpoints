using System;

namespace Relativity.Sync.Configuration
{
	internal interface ISnapshotPartitionConfiguration : IConfiguration
	{
		int TotalRecordsCount { get; }

		int BatchSize { get; }
		
		int SourceWorkspaceArtifactId { get; }

		int SyncConfigurationArtifactId { get; }

        Guid ExportRunId { get; }
	}
}