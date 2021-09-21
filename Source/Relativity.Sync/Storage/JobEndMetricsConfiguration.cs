using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class JobEndMetricsConfiguration : IJobEndMetricsConfiguration
	{
		private readonly SyncJobParameters _syncJobParameters;
		private readonly IConfiguration _cache;

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
		public int? JobHistoryToRetryId => _cache.GetFieldValue(x => x.JobHistoryToRetryId);

		public ImportOverwriteMode ImportOverwriteMode => _cache.GetFieldValue(x => x.ImportOverwriteMode);
		public DataSourceType DataSourceType =>  _cache.GetFieldValue(x => x.DataSourceType);
		public DestinationLocationType DestinationType => _cache.GetFieldValue(x => x.DataDestinationType);
		public ImportNativeFileCopyMode ImportNativeFileCopyMode => _cache.GetFieldValue(x => x.NativesBehavior);
		public ImportImageFileCopyMode ImportImageFileCopyMode => _cache.GetFieldValue(x => x.ImageFileCopyMode);

		public JobEndMetricsConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_syncJobParameters = syncJobParameters;
			_cache = cache;
		}
	}
}