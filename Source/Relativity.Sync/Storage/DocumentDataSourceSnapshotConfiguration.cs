using System;
using Relativity.Sync.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Caching;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class DocumentDataSourceSnapshotConfiguration : IDocumentDataSourceSnapshotConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly IFieldMappings _fieldMappings;
		private readonly SyncJobParameters _syncJobParameters;

		public DocumentDataSourceSnapshotConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_fieldMappings = fieldMappings;
			_syncJobParameters = syncJobParameters;
		}

		public IList<FieldMap> GetFieldMappings() => _fieldMappings.GetFieldMappings();
		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

		public int DataSourceArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.DataSourceArtifactIdGuid);

		public bool IsSnapshotCreated => !string.IsNullOrWhiteSpace(_cache.GetFieldValue<string>(SyncConfigurationRdo.SnapshotIdGuid));

		public async Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount)
		{
			await _cache.UpdateFieldValueAsync(SyncConfigurationRdo.SnapshotIdGuid, runId.ToString()).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(SyncConfigurationRdo.SnapshotRecordsCountGuid, totalRecordsCount).ConfigureAwait(false);
		}
	}
}
