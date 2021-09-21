namespace Relativity.Sync.Configuration
{
	internal interface IDocumentJobStartMetricsConfiguration : IConfiguration
	{
		bool Resuming { get; }

		int? JobHistoryToRetryId { get; }

		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }

		ImportNativeFileCopyMode ImportNativeFileCopyMode { get; }
	}
}
