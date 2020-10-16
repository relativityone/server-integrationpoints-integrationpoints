namespace Relativity.Sync.Configuration
{
	internal interface ISumReporterConfiguration : IConfiguration
	{
		int? JobHistoryToRetryId { get; }

		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }
	}
}