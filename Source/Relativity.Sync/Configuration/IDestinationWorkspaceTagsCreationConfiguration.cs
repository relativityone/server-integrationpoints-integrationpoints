﻿namespace Relativity.Sync.Configuration
{
	internal interface IDestinationWorkspaceTagsCreationConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }

		int JobArtifactId { get; }

		int SourceWorkspaceArtifactTypeId { get; }

		int SourceJobArtifactTypeId { get; }

		bool IsSourceJobTagSet { get; }

		void SetSourceJobTag(int artifactId, string name);

		bool IsSourceWorkspaceTagSet { get; }

		void SetSourceWorkspaceTag(int artifactId, string name);
	}
}