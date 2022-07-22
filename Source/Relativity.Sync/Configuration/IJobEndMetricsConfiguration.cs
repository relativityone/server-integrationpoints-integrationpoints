using System;

namespace Relativity.Sync.Configuration
{
    internal interface IJobEndMetricsConfiguration : IConfiguration
    {
        int? JobHistoryToRetryId { get; }

        int SourceWorkspaceArtifactId { get; }

        int DestinationWorkspaceArtifactId { get; }

        int SyncConfigurationArtifactId { get; }

        ImportOverwriteMode ImportOverwriteMode { get; }

        DataSourceType DataSourceType { get; }

        DestinationLocationType DestinationType { get; }

        ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }

        ImportImageFileCopyMode ImportImageFileCopyMode { get; }

        Guid ExportRunId { get; }
    }
}