using System;

namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		int DestinationWorkspaceTagArtifactId { get; }

		Guid ExportRunId { get; }

		ImportSettingsDto ImportSettings { get; }

		int JobHistoryArtifactId { get; }

		string SourceJobTagName { get; }

		int SourceWorkspaceArtifactId { get; }

		string SourceWorkspaceTagName { get; }

		int SyncConfigurationArtifactId { get; }

		DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; }
	}
}