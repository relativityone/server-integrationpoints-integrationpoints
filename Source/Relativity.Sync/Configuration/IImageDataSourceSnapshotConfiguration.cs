using System.Collections.Generic;
using System.Diagnostics;

namespace Relativity.Sync.Configuration
{
	internal interface IImageDataSourceSnapshotConfiguration : IDataSourceSnapshotConfiguration
	{
		int[] ProductionImagePrecedence { get; }

		bool IncludeOriginalImageIfNotFoundInProductions { get; }
	}
}
