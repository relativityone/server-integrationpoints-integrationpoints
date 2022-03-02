using System;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class SnapshotPartitionConfiguration : ISnapshotPartitionConfiguration
	{
		private readonly IInstanceSettings _instanceSettings;
		private readonly ISyncLog _syncLog;

		protected readonly IConfiguration Cache;

        public SnapshotPartitionConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, IInstanceSettings instanceSettings, ISyncLog syncLog)
		{
			Cache = cache;
            _instanceSettings = instanceSettings;
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

        public async Task<int> GetSyncBatchSizeAsync()
        {
            return await _instanceSettings.GetSyncBatchSizeAsync().ConfigureAwait(false);
        }
    }
}