namespace Relativity.Sync.Configuration
{
	internal interface IDocumentJobStartMetricsConfiguration : IConfiguration
	{
		int? JobHistoryToRetryId { get; }

		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }
	}
}
