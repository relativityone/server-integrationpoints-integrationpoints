using System;

namespace Relativity.Sync.Configuration
{
    internal interface IDocumentSynchronizationMonitorConfiguration : IConfiguration
    {
        public int DestinationWorkspaceArtifactId { get; }

        public Guid ExportRunId { get; }
    }
}
