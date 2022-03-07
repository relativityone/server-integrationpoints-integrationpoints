using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Configuration
{
	internal interface ISnapshotPartitionConfiguration : IConfiguration
	{
		int TotalRecordsCount { get; }

		int SourceWorkspaceArtifactId { get; }

		int SyncConfigurationArtifactId { get; }

        Guid ExportRunId { get; }

		Task<int> GetSyncBatchSizeAsync();
	}
}