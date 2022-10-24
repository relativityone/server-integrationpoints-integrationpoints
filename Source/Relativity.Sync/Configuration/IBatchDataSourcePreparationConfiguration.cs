using System;

namespace Relativity.Sync.Configuration
{
    internal interface IBatchDataSourcePreparationConfiguration : IConfiguration
    {
        public int SourceWorkspaceArtifactId { get; }

        public int SyncConfigurationArtifactId { get; }

        public Guid ExportRunId { get; }

        public int DestinationWorkspaceArtifactId { get; }

        public int JobHistoryId { get; }
    }
}
