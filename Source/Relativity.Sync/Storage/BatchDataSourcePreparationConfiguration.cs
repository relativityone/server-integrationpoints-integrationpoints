using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class BatchDataSourcePreparationConfiguration : IBatchDataSourcePreparationConfiguration
    {
        private const int _ASCII_GROUP_SEPARATOR = 29;
        private const int _ASCII_RECORD_SEPARATOR = 30;

        private readonly IConfiguration _cache;
        private readonly SyncJobParameters _parameters;

        public BatchDataSourcePreparationConfiguration(IConfiguration cache, SyncJobParameters jobParameters)
        {
            _cache = cache;
            _parameters = jobParameters;
        }

        public int SourceWorkspaceArtifactId => _parameters.WorkspaceId;

        public int SyncConfigurationArtifactId => _parameters.SyncConfigurationArtifactId;

        public Guid ExportRunId
        {
            get
            {
                Guid? snapshotId = _cache.GetFieldValue(x => x.SnapshotId);
                if (snapshotId == Guid.Empty)
                {
                    snapshotId = null;
                }

                return snapshotId ?? throw new ArgumentException($"Run ID needs to be valid GUID, but null found.");
            }
        }

        public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
    }
}
