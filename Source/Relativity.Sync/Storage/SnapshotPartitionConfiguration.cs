using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class SnapshotPartitionConfiguration : ISnapshotPartitionConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly ISyncLog _syncLog;

		public SnapshotPartitionConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, SyncJobExecutionConfiguration configuration, ISyncLog syncLog)
		{
			_cache = cache;
			_syncLog = syncLog;

			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
			SyncConfigurationArtifactId = syncJobParameters.SyncConfigurationArtifactId;
			BatchSize = configuration.BatchSize;
		}

		public int TotalRecordsCount => _cache.GetFieldValue(x => x.SnapshotRecordsCount);

		public int BatchSize { get; }

		public Guid ExportRunId
		{
			get
			{
				Guid? snapshotId = _cache.GetFieldValue(x => x.SnapshotId);
				if (snapshotId == Guid.Empty)
				{
					snapshotId = null;
				}

				return snapshotId ?? throw new ArgumentException($"Run ID needs to be valid GUID, but null found.");
			}
		}

		public int SourceWorkspaceArtifactId { get; }
		public int SyncConfigurationArtifactId { get; }
	}
}