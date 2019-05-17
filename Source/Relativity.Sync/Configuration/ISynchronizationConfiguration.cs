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

		Guid ExportRunId { get; }

		IList<FieldMap> FieldMappings { get; }

		int SyncConfigurationId { get; }
	}
}