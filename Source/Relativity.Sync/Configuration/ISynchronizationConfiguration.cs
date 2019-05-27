﻿using System;
using System.Collections.Generic;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Configuration
{
	internal interface ISynchronizationConfiguration : IConfiguration
	{
		int DestinationWorkspaceTagArtifactId { get; }

		Guid ExportRunId { get; }

		IList<FieldMap> FieldMappings { get; }

		ImportSettingsDto ImportSettings { get; }

		int JobHistoryArtifactId { get; }

		int JobHistoryTagArtifactId { get; }

		string SourceJobTagName { get; }

		int SourceWorkspaceArtifactId { get; }

		string SourceWorkspaceTagName { get; }

		int SyncConfigurationArtifactId { get; }
	}
}