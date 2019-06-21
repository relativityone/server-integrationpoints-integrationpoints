using System;

namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		int DestinationWorkspaceArtifactId { get; }

		int DestinationWorkspaceTagArtifactId { get; }

		Guid ExportRunId { get; }

		ImportSettingsDto ImportSettings { get; }

		int JobHistoryArtifactId { get; }

		int SourceJobTagArtifactId { get; }

		int SourceWorkspaceArtifactId { get; }

		int SourceWorkspaceTagArtifactId { get; }

		int SyncConfigurationArtifactId { get; }

		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }
	}
}