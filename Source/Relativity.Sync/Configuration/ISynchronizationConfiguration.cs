using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		int SyncConfigurationArtifactId { get; }

		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceTagArtifactId { get; }

		int JobHistoryTagArtifactId { get; }

		ImportSettingsDto ImportSettings { get; }
	}
}