﻿namespace Relativity.Sync.Configuration
{
	internal interface ISnapshotQueryConfiguration
	{
		int? JobHistoryToRetryId { get; }

		int DataSourceArtifactId { get; }

		int workspaceId { get; }

		int[] ProductionImagePrecedence { get; }

		bool IncludeOriginalImageIfNotFoundInProductions { get; }
	}
}
