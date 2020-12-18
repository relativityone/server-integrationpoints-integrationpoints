using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class JobEndMetricsConfiguration : IJobEndMetricsConfiguration
	{
		private readonly SyncJobParameters _syncJobParameters;
		private readonly IConfiguration _cache;

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid);
		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
		public int? JobHistoryToRetryId => _cache.GetFieldValue<RelativityObjectValue>(SyncConfigurationRdo.JobHistoryToRetryGuid)?.ArtifactID;

		public JobEndMetricsConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_syncJobParameters = syncJobParameters;
			_cache = cache;
		}
	}
}