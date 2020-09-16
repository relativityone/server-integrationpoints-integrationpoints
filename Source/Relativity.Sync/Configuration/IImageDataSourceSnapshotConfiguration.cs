using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	internal interface IImageDataSourceSnapshotConfiguration : IDataSourceSnapshotConfiguration
	{
		int[] ProductionIds { get; }
		bool IncludeOriginalImageIfNotFoundInProductions { get; }
	}
}
