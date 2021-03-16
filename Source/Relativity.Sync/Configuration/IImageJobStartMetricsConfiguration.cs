namespace Relativity.Sync.Configuration
{
	interface IImageJobStartMetricsConfiguration : IConfiguration
	{
		int? JobHistoryToRetryId { get; }

		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }

		int[] ProductionImagePrecedence { get; }

		bool IncludeOriginalImageIfNotFoundInProductions { get; }
	}
}
