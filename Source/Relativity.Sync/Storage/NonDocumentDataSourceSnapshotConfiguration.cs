using Relativity.Sync.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal class NonDocumentDataSourceSnapshotConfiguration : INonDocumentDataSourceSnapshotConfiguration
	{
		private readonly IFieldMappings _fieldMappings;
		private readonly SyncJobParameters _syncJobParameters;

		protected readonly IConfiguration Cache;

		public NonDocumentDataSourceSnapshotConfiguration(IFieldMappings fieldMappings, SyncJobParameters syncJobParameters, IConfiguration cache)
		{
			_fieldMappings = fieldMappings;
			_syncJobParameters = syncJobParameters;
			Cache = cache;
		}

		public IList<FieldMap> GetFieldMappings() => _fieldMappings.GetFieldMappings();

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

		public int DataSourceArtifactId => Cache.GetFieldValue(x => x.DataSourceArtifactId);

		public int RdoArtifactTypeId => Cache.GetFieldValue(x => x.RdoArtifactTypeId);

		public bool IsSnapshotCreated
		{
			get
			{
				Guid? fieldValue = Cache.GetFieldValue(x => x.SnapshotId);
				return fieldValue != null &&
					   !fieldValue.Equals(Guid.Empty);
			}
		}

		public async Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount)
		{
			await Cache.UpdateFieldValueAsync(x => x.SnapshotId, runId).ConfigureAwait(false);
			await Cache.UpdateFieldValueAsync(x => x.SnapshotRecordsCount, totalRecordsCount).ConfigureAwait(false);
		}

		public async Task SetObjectLinkingSnapshotDataAsync(Guid objectLinkingSnapshotId, int objectLinkingRecordsCount)
		{
			await Cache.UpdateFieldValueAsync(x => x.ObjectLinkingSnapshotId, objectLinkingSnapshotId).ConfigureAwait(false);
			await Cache.UpdateFieldValueAsync(x => x.ObjectLinkingSnapshotRecordsCount, objectLinkingRecordsCount).ConfigureAwait(false);
		}
	}
}
