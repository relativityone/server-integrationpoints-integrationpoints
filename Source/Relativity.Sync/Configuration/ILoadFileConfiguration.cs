using System;

namespace Relativity.Sync.Configuration
{
    internal interface ILoadFileConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }

        Guid ExportRunId { get; }
    }
}
