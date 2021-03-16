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

		public ImageDataSourceSnapshotConfiguration(IConfiguration cache, ISerializer serializer, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_serializer = serializer;
			_syncJobParameters = syncJobParameters;
		}

		public int[] ProductionImagePrecedence => _serializer.Deserialize<int[]>(_cache.GetFieldValue(x => x.ProductionImagePrecedence));

		public bool IncludeOriginalImageIfNotFoundInProductions =>
			_cache.GetFieldValue(x => x.IncludeOriginalImages);

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

		public int DataSourceArtifactId => _cache.GetFieldValue(x => x.DataSourceArtifactId);

		public bool IsSnapshotCreated => !string.IsNullOrWhiteSpace(_cache.GetFieldValue(x => x.SnapshotId));

		public async Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount)
		{
			await _cache.UpdateFieldValueAsync(x => x.SnapshotId, runId.ToString()).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(x => x.SnapshotRecordsCount, totalRecordsCount).ConfigureAwait(false);
		}
	}
}
