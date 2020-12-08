using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class ImageSyncOptions
	{
		public ImportImageFileCopyMode CopyImagesMode { get; set; }
		public DestinationLocationType DestinationLocationType { get; set; }
		public int DestinationLocationId { get; set; }
		public DataSourceType DataSourceType { get; set; }
		public int DataSourceId { get; set; }

		public ImageSyncOptions(DataSourceType dataSourceType, int dataSourceId, 
			DestinationLocationType destinationLocationType, int destinationLocationId,
			ImportImageFileCopyMode copyImagesMode = ImportImageFileCopyMode.SetFileLinks)
		{
			DataSourceType = dataSourceType;
			DataSourceId = dataSourceId;
			DestinationLocationType = destinationLocationType;
			DestinationLocationId = destinationLocationId;
			CopyImagesMode = copyImagesMode;
		}
	}
}
