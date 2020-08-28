using Relativity.Sync.Configuration;
using System;
using System.Collections.Generic;

namespace Relativity.Sync.Storage
{
	internal sealed class ImageDataSourceSnapshotConfiguration : DataSourceSnapshotConfigurationBase,
		IImageDataSourceSnapshotConfiguration, IImageRetryDataSourceSnapshotConfiguration
	{
		public ImageDataSourceSnapshotConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		: base(cache, syncJobParameters)
		{
		}

		public List<int> ProductionIds => throw new NotImplementedException();

		public bool IncludeOriginalImageIfNotFoundInProductions => throw new NotImplementedException();
	}
}
