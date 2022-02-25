namespace Relativity.Sync.Configuration
{
	internal interface INonDocumentJobStartMetricsConfiguration : IConfiguration
	{
        bool Resuming { get; }

        int? JobHistoryToRetryId { get; }

        int SourceWorkspaceArtifactId { get; }

        int DestinationWorkspaceArtifactId { get; }
	}
}
