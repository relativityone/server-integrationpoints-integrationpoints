namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		int DestinationWorkspaceTagArtifactId { get; }

		int JobHistoryTagArtifactId { get; }

		ImportSettingsDto ImportSettings { get; }
		int SyncConfigurationArtifactId { get; }
	}
}