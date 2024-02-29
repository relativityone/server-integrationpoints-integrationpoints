using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class JobStatusConsolidationConfiguration : IJobStatusConsolidationConfiguration
    {
        private readonly IConfiguration _cache;
        private readonly SyncJobParameters _syncJobParameters;

        public JobStatusConsolidationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
        {
            _cache = cache;
            _syncJobParameters = syncJobParameters;
        }

        public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

        public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;

        public int JobHistoryArtifactId => _cache.GetFieldValue(x => x.JobHistoryId);

        public Guid? ExportRunId => _cache.GetFieldValue(x => x.SnapshotId);
    }
}
