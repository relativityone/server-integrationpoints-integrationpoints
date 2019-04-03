﻿using System;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class DataSourceSnapshotConfiguration : IDataSourceSnapshotConfiguration
	{
		private readonly IConfiguration _cache;

		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid FieldMappingsGuid = new Guid("E3CB5C64-C726-47F8-9CB0-1391C5911628");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");

		public DataSourceSnapshotConfiguration(IConfiguration cache, int sourceWorkspaceArtifactId)
		{
			_cache = cache;
			SourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
		}

		public int SourceWorkspaceArtifactId { get; }

		public int DataSourceArtifactId => _cache.GetFieldValue<int>(DataSourceArtifactIdGuid);

		public string FieldMappings => _cache.GetFieldValue<string>(FieldMappingsGuid);

		public bool IsSnapshotCreated => !string.IsNullOrWhiteSpace(_cache.GetFieldValue<string>(SnapshotIdGuid));

		public async Task SetSnapshotDataAsync(Guid runId, long totalRecordsCount)
		{
			await _cache.UpdateFieldValueAsync(SnapshotIdGuid, runId.ToString()).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(SnapshotRecordsCountGuid, totalRecordsCount).ConfigureAwait(false);
		}
	}
}