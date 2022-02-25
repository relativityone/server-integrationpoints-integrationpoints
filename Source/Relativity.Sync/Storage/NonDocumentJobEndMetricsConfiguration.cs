using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class NonDocumentJobEndMetricsConfiguration : INonDocumentJobEndMetricsConfiguration
	{
        private readonly SyncJobParameters _syncJobParameters;
        private readonly IConfiguration _cache;

        public bool Resuming => _cache.GetFieldValue(x => x.Resuming);
        public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
        public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
        public int? JobHistoryToRetryId => _cache.GetFieldValue(x => x.JobHistoryToRetryId);

        public NonDocumentJobEndMetricsConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
        {
            _syncJobParameters = syncJobParameters;
            _cache = cache;
        }
    }
}
