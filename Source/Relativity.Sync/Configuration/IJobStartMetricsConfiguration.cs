namespace Relativity.Sync.Configuration
{
    internal interface IJobStartMetricsConfiguration : IConfiguration
    {
        bool Resuming { get; }

        int? JobHistoryToRetryId { get; }

        int SourceWorkspaceArtifactId { get; }

        int DestinationWorkspaceArtifactId { get; }
    }
}
