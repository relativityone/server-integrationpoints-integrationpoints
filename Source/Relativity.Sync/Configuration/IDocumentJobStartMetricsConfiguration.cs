namespace Relativity.Sync.Configuration
{
	interface IDocumentJobStartMetricsConfiguration : IConfiguration
	{
		int? JobHistoryToRetryId { get; }

		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }
	}
}
