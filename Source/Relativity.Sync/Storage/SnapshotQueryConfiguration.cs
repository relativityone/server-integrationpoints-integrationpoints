using Relativity.Sync.Configuration;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
	internal class SnapshotQueryConfiguration : ISnapshotQueryConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly ISerializer _serializer;
		private readonly SyncJobParameters _syncJobParameters;

		public SnapshotQueryConfiguration(IConfiguration cache, ISerializer serializer, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_serializer = serializer;
			_syncJobParameters = syncJobParameters;
		}

		public int? JobHistoryToRetryId => _cache.GetFieldValue(x => x.JobHistoryToRetryId);

		public int DataSourceArtifactId => _cache.GetFieldValue(x => x.DataSourceArtifactId);

		public int workspaceId => _syncJobParameters.WorkspaceId;

		public int[] ProductionImagePrecedence => _serializer.Deserialize<int[]>(_cache.GetFieldValue(x => x.ProductionImagePrecedence));

		public bool IncludeOriginalImageIfNotFoundInProductions => _cache.GetFieldValue(x => x.IncludeOriginalImages);
	}
}
