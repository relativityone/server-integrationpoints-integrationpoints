using System;

namespace Relativity.Sync.Configuration
{
    internal interface IJobCleanupConfiguration : IConfiguration
    {
        int SourceWorkspaceArtifactId { get; }

        int DestinationWorkspaceArtifactId { get; }

        int SyncConfigurationArtifactId { get; }

        Guid ExportRunId { get; }
    }
}
