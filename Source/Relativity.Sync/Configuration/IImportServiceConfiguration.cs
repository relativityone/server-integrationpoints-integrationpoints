using System;

namespace Relativity.Sync.Configuration
{
    internal interface IImportServiceConfiguration : IConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }

        Guid ExportRunId { get; }
    }
}
