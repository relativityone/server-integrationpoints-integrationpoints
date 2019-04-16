using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class SnapshotPartitionConfiguration : ISnapshotPartitionConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly ISyncLog _syncLog;

		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");

		public SnapshotPartitionConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, ISyncLog syncLog)
		{
			_cache = cache;
			_syncLog = syncLog;

			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
			SyncConfigurationArtifactId = syncJobParameters.JobId;
		}

		public int TotalRecordsCount => _cache.GetFieldValue<int>(SnapshotRecordsCountGuid);

		public Guid ExportRunId
		{
			get
			{
				string runId = _cache.GetFieldValue<string>(SnapshotIdGuid);
				Guid guid;
				if (Guid.TryParse(runId, out guid))
				{
					return guid;
				}

				_syncLog.LogError("Unable to parse export run ID {runId}.", runId);
				return Guid.Empty;
			}
		}

		public int SourceWorkspaceArtifactId { get; }
		public int SyncConfigurationArtifactId { get; }
	}
}