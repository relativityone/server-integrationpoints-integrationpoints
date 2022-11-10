using System;

namespace Relativity.Sync.Configuration
{
    internal interface IDocumentSynchronizationMonitorConfiguration : IConfiguration
    {
        public int SourceWorkspaceArtifactId { get; }

        public int DestinationWorkspaceArtifactId { get; }

        public int JobHistoryArtifactId { get; }

        public Guid ExportRunId { get; }
    }
}
