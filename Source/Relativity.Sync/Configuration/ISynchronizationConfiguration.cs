using System;
using System.Collections.Generic;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceTagArtifactId { get; }

		int JobHistoryTagArtifactId { get; }

		ImportSettingsDto ImportSettings { get; }

		IList<FieldMap> FieldMappings { get; }

		int SyncConfigurationArtifactId { get; }

		Guid ExportRunId { get; }
	}
}