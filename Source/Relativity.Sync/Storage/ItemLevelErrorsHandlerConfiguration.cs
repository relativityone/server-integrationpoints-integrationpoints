using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class ItemLevelErrorsHandlerConfiguration : IItemLevelErrorHandlerConfiguration
    {
        private readonly IConfiguration _cache;
        private readonly SyncJobParameters _parameters;

        public ItemLevelErrorsHandlerConfiguration(IConfiguration cache, SyncJobParameters jobParameters)
        {
            _cache = cache;
            _parameters = jobParameters;
        }

        public int SourceWorkspaceArtifactId => _parameters.WorkspaceId;

        public int JobHistoryArtifactId => _cache.GetFieldValue(x => x.JobHistoryId);
    }
}
