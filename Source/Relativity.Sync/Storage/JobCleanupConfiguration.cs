using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal sealed class JobCleanupConfiguration : IJobCleanupConfiguration
    {
        private readonly IConfiguration _cache;
        private readonly SyncJobParameters _syncJobParameters;

        public JobCleanupConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
        {
            _cache = cache;
            _syncJobParameters = syncJobParameters;
        }

        public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

        public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);

        public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;

        public Guid ExportRunId
        {
            get
            {
                Guid snapshotId = _cache.GetFieldValue(x => x.SnapshotId) ?? Guid.Empty;
                return snapshotId != Guid.Empty
                    ? snapshotId
                    : throw new ArgumentException($"Run ID needs to be valid GUID, but null found.");
            }
        }
    }
}
