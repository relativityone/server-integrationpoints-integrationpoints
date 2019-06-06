using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class DataSourceSnapshotConfiguration : IDataSourceSnapshotConfiguration, IFieldConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly IFieldMappings _fieldMappings;

		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");
		private static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");
		private static readonly Guid FolderPathSourceFieldNameGuid = new Guid("66A37443-EF92-47ED-BEEA-392464C853D3");

		public DataSourceSnapshotConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_fieldMappings = fieldMappings;
			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
		}

		public string FolderPathSourceFieldName => _cache.GetFieldValue<string>(FolderPathSourceFieldNameGuid);

		public int SourceWorkspaceArtifactId { get; }

		public int DataSourceArtifactId => _cache.GetFieldValue<int>(DataSourceArtifactIdGuid);
		public IList<FieldMap> FieldMappings => _fieldMappings.GetFieldMappings();

		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior =>
			(DestinationFolderStructureBehavior) Enum.Parse(typeof(DestinationFolderStructureBehavior), _cache.GetFieldValue<string>(DestinationFolderStructureBehaviorGuid));
		
		public bool IsSnapshotCreated => !string.IsNullOrWhiteSpace(_cache.GetFieldValue<string>(SnapshotIdGuid));

		public async Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount)
		{
			await _cache.UpdateFieldValueAsync(SnapshotIdGuid, runId.ToString()).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(SnapshotRecordsCountGuid, totalRecordsCount).ConfigureAwait(false);
		}
	}
}