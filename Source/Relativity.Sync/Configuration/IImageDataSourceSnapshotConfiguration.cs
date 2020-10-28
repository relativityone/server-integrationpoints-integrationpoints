using System.Collections.Generic;

namespace Relativity.Sync.Configuration
{
	internal interface IImageDataSourceSnapshotConfiguration : IDataSourceSnapshotConfiguration
	{
		int[] ProductionImagePrecedence { get; }
		bool IncludeOriginalImageIfNotFoundInProductions { get; }
	}
}
