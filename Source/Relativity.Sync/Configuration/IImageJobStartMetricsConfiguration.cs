﻿namespace Relativity.Sync.Configuration
{
	internal interface IImageJobStartMetricsConfiguration : IConfiguration
	{
		int? JobHistoryToRetryId { get; }

		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }

		int[] ProductionImagePrecedence { get; }

		bool IncludeOriginalImageIfNotFoundInProductions { get; }
	}
}
