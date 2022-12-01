using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class IAPIv2RunCheckerConfiguration : IIAPIv2RunCheckerConfiguration
    {
        private readonly IConfiguration _cache;
        private readonly SyncJobParameters _jobParameters;

        public IAPIv2RunCheckerConfiguration(IConfiguration cache, SyncJobParameters jobParameters)
        {
            _cache = cache;
            _jobParameters = jobParameters;
        }

        public ImportNativeFileCopyMode NativeBehavior => _cache.GetFieldValue(x => x.NativesBehavior);

        public bool ImageImport => _cache.GetFieldValue(x => x.ImageImport);

        public int RdoArtifactTypeId => _cache.GetFieldValue(x => x.RdoArtifactTypeId);

        public bool IsRetried => _cache.GetFieldValue(x => x.JobHistoryToRetryId).HasValue && _cache.GetFieldValue(x => x.JobHistoryToRetryId) > 0;

        public bool IsDrainStopped => _cache.GetFieldValue(x => x.Resuming);

        public int SourceWorkspaceArtifactId => _jobParameters.WorkspaceId;
    }
}
