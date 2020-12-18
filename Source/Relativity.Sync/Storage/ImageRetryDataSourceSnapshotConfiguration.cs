using System;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
	internal sealed class ImageRetryDataSourceSnapshotConfiguration : IImageRetryDataSourceSnapshotConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly ISerializer _serializer;
		private readonly SyncJobParameters _syncJobParameters;

		public ImageRetryDataSourceSnapshotConfiguration(IConfiguration cache, ISerializer serializer, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_serializer = serializer;
			_syncJobParameters = syncJobParameters;
		}

		public int[] ProductionImagePrecedence => _serializer.Deserialize<int[]>(_cache.GetFieldValue<string>(SyncConfigurationRdo.ProductionImagePrecedenceGuid));

		public bool IncludeOriginalImageIfNotFoundInProductions =>
			_cache.GetFieldValue<bool>(SyncConfigurationRdo.IncludeOriginalImagesGuid);

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

		public int DataSourceArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.DataSourceArtifactIdGuid);

		public bool IsSnapshotCreated => !string.IsNullOrWhiteSpace(_cache.GetFieldValue<string>(SyncConfigurationRdo.SnapshotIdGuid));

		public async Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount)
		{
			await _cache.UpdateFieldValueAsync(SyncConfigurationRdo.SnapshotIdGuid, runId.ToString()).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(SyncConfigurationRdo.SnapshotRecordsCountGuid, totalRecordsCount).ConfigureAwait(false);
		}

		public int? JobHistoryToRetryId => _cache.GetFieldValue<RelativityObjectValue>(SyncConfigurationRdo.JobHistoryToRetryGuid)?.ArtifactID;

		public ImportOverwriteMode ImportOverwriteMode
		{
			get => (ImportOverwriteMode)(Enum.Parse(typeof(ImportOverwriteMode), _cache.GetFieldValue<string>(SyncConfigurationRdo.ImportOverwriteModeGuid)));
			set => _cache.UpdateFieldValueAsync(SyncConfigurationRdo.ImportOverwriteModeGuid, value.ToString());
		}
	}
}
