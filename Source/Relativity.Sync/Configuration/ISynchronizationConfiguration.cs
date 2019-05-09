namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		int DestinationWorkspaceTagArtifactId { get; }

		int JobHistoryTagArtifactId { get; }

		int SourceWorkspaceArtifactId { get; }

		int SyncConfigurationArtifactId { get; }
	}
}