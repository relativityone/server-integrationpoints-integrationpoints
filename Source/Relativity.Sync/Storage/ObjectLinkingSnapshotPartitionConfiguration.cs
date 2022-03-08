using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class ObjectLinkingSnapshotPartitionConfiguration : SnapshotPartitionConfiguration, IObjectLinkingSnapshotPartitionConfiguration
	{
		public ObjectLinkingSnapshotPartitionConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, IInstanceSettings instanceSettings, ISyncLog syncLog)
			: base(cache, syncJobParameters, instanceSettings, syncLog)
		{
		}

		public override Guid ExportRunId
		{
			get
			{
				Guid? snapshotId = Cache.GetFieldValue(x => x.ObjectLinkingSnapshotId);
				if (snapshotId == Guid.Empty)
				{
					snapshotId = null;
				}

				return snapshotId ?? throw new ArgumentException($"ObjectLinkingSnapshotId needs to be valid GUID, but null found.");
			}
		}

		public bool LinkingExportExists => Cache.GetFieldValue(x => x.ObjectLinkingSnapshotId).HasValue;
	}
}