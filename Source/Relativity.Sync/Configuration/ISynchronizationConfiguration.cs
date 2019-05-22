using System;
using System.Collections.Generic;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }

		int DestinationWorkspaceTagArtifactId { get; }

		int JobHistoryTagArtifactId { get; }

		ImportSettingsDto ImportSettings { get; }

		Guid ExportRunId { get; }

		IList<FieldMap> FieldMappings { get; }

		int SyncConfigurationArtifactId { get; }

		void SetImportSettings(ImportSettingsDto importSettings);

		string SourceJobTagName { get; }

		string SourceWorkspaceTagName { get; }
	}
}