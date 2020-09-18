using Relativity.Sync.Configuration;
using System;
using System.Threading.Tasks;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
	internal sealed class ImageDataSourceSnapshotConfiguration : IImageDataSourceSnapshotConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly ISerializer _serializer;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");
		private static readonly Guid IncludeOriginalImagesGuid = new Guid("F2CAD5C5-63D5-49FC-BD47-885661EF1D8B");
		private static readonly Guid ProductionImagePrecedenceGuid = new Guid("421CF05E-BAB4-4455-A9CA-FA83D686B5ED");

		public ImageDataSourceSnapshotConfiguration(IConfiguration cache, ISerializer serializer, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_serializer = serializer;
			_syncJobParameters = syncJobParameters;
		}

		public int[] ProductionIds => _serializer.Deserialize<int[]>(_cache.GetFieldValue<string>(ProductionImagePrecedenceGuid));

		public bool IncludeOriginalImageIfNotFoundInProductions =>
			_cache.GetFieldValue<bool>(IncludeOriginalImagesGuid);

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

		public int DataSourceArtifactId => _cache.GetFieldValue<int>(DataSourceArtifactIdGuid);

		public bool IsSnapshotCreated => !string.IsNullOrWhiteSpace(_cache.GetFieldValue<string>(SnapshotIdGuid));

		public async Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount)
		{
			await _cache.UpdateFieldValueAsync(SnapshotIdGuid, runId.ToString()).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(SnapshotRecordsCountGuid, totalRecordsCount).ConfigureAwait(false);
		}
	}
}
