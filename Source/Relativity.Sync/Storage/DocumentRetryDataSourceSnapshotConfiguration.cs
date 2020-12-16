using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class DocumentRetryDataSourceSnapshotConfiguration : IDocumentRetryDataSourceSnapshotConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly IFieldMappings _fieldMappings;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");
		private static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");

		public DocumentRetryDataSourceSnapshotConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_fieldMappings = fieldMappings;
			_syncJobParameters = syncJobParameters;
		}
		public IList<FieldMap> GetFieldMappings() => _fieldMappings.GetFieldMappings();
		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

		public int DataSourceArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.DataSourceArtifactIdGuid);

		public bool IsSnapshotCreated => !string.IsNullOrWhiteSpace(_cache.GetFieldValue<string>(SnapshotIdGuid));

		public async Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount)
		{
			await _cache.UpdateFieldValueAsync(SnapshotIdGuid, runId.ToString()).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(SnapshotRecordsCountGuid, totalRecordsCount).ConfigureAwait(false);
		}

		public int? JobHistoryToRetryId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryToRetryGuid)?.ArtifactID;

		public ImportOverwriteMode ImportOverwriteMode
		{
			get => (ImportOverwriteMode)(Enum.Parse(typeof(ImportOverwriteMode), _cache.GetFieldValue<string>(ImportOverwriteModeGuid)));
			set => _cache.UpdateFieldValueAsync(ImportOverwriteModeGuid, value.ToString());
		}
	}
}
