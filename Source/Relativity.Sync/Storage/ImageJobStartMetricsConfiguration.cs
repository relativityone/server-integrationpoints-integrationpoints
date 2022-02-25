using Relativity.Sync.Configuration;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
	internal class ImageJobStartMetricsConfiguration : IImageJobStartMetricsConfiguration
	{
		private readonly SyncJobParameters _syncJobParameters;
		private readonly IConfiguration _cache;
		private readonly ISerializer _serializer;

		public bool Resuming => _cache.GetFieldValue(x => x.Resuming);
		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int? JobHistoryToRetryId => _cache.GetFieldValue(x => x.JobHistoryToRetryId);

		public int[] ProductionImagePrecedence => _serializer.Deserialize<int[]>(_cache.GetFieldValue(x => x.ProductionImagePrecedence));

		public bool IncludeOriginalImageIfNotFoundInProductions =>
			_cache.GetFieldValue(x => x.IncludeOriginalImages);

        public ImageJobStartMetricsConfiguration(IConfiguration cache, ISerializer serializer, SyncJobParameters syncJobParameters)
		{
			_syncJobParameters = syncJobParameters;
			_cache = cache;
			_serializer = serializer;
		}
	}
}
