using System;

namespace Relativity.Sync.Configuration
{
    internal interface IJobStatusConsolidationConfiguration : IConfiguration
    {
        int SourceWorkspaceArtifactId { get; }

        int SyncConfigurationArtifactId { get; }

        int JobHistoryArtifactId { get; }

        Guid? ExportRunId { get; }
    }
}