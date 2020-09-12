using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	internal interface IImageDataSourceSnapshotConfiguration : IDataSourceSnapshotConfiguration
	{
		List<int> ProductionIds { get; }
		bool IncludeOriginalImageIfNotFoundInProductions { get; }
		bool IsImageJob { get; }
	}
}
