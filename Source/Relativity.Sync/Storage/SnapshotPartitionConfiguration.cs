using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class SnapshotPartitionConfiguration : ISnapshotPartitionConfiguration
	{
		private readonly ISyncLog _syncLog;

		protected readonly IConfiguration Cache;

		public SnapshotPartitionConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, ISyncLog syncLog)
		{
			Cache = cache;
			_syncLog = syncLog;

			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
			SyncConfigurationArtifactId = syncJobParameters.SyncConfigurationArtifactId;
		}

		public int TotalRecordsCount => Cache.GetFieldValue(x => x.SnapshotRecordsCount);

		public virtual Guid ExportRunId
		{
			get
			{
				Guid? snapshotId = Cache.GetFieldValue(x => x.SnapshotId);
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