using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		int SyncConfigurationArtifactId { get; }

		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceTagArtifactId { get; }

		int JobHistoryTagArtifactId { get; }

		ImportOverwriteMode ImportOverwriteMode { get; }

		FieldOverlayBehavior FieldOverlayBehavior { get; }

		bool SendEmails { get; }

		int DestinationFolderArtifactId { get; }
	}
}