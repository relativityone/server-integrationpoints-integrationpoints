using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class DataSourceSnapshotConfiguration : IDataSourceSnapshotConfiguration
	{
		private readonly IFieldMappings _fieldMappings;
		private readonly SyncJobParameters _syncJobParameters;

		protected readonly IConfiguration Cache;

		public DataSourceSnapshotConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters)
		{
			Cache = cache;
			_fieldMappings = fieldMappings;
			_syncJobParameters = syncJobParameters;
		}

		public IList<FieldMap> GetFieldMappings() => _fieldMappings.GetFieldMappings();

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

		public int DataSourceArtifactId => Cache.GetFieldValue(x => x.DataSourceArtifactId);

		public bool IsSnapshotCreated => !string.IsNullOrWhiteSpace(Cache.GetFieldValue(x => x.SnapshotId));

		public async Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount)
		{
			await Cache.UpdateFieldValueAsync(x => x.SnapshotId, runId.ToString()).ConfigureAwait(false);
			await Cache.UpdateFieldValueAsync(x => x.SnapshotRecordsCount, totalRecordsCount).ConfigureAwait(false);
		}
	}
}
