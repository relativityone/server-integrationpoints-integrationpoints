using System;

namespace Relativity.Sync.Configuration
{
	internal interface IImageJobStartMetricsConfiguration : IConfiguration
	{
		bool Resuming { get; }

		int? JobHistoryToRetryId { get; }

		int SourceWorkspaceArtifactId { get; }

		int DestinationWorkspaceArtifactId { get; }

		int[] ProductionImagePrecedence { get; }

		bool IncludeOriginalImageIfNotFoundInProductions { get; }
    }
}
