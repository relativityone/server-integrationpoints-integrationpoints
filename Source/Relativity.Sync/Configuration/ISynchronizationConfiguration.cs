﻿namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceTagArtifactId { get; }

		int JobHistoryTagArtifactId { get; }

		ImportSettingsDto ImportSettings { get; }

		int SourceWorkspaceArtifactId { get; }

		int SyncConfigurationArtifactId { get; }
	}
}