using Relativity.Sync.Configuration;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class ImageSyncOptions
	{
		public DestinationLocationType DestinationLocationType { get; }

		public int DestinationLocationId { get; }

		public DataSourceType DataSourceType { get; }

		public int DataSourceId { get; }

		public ImportImageFileCopyMode CopyImagesMode { get; set; }

		public ImageSyncOptions(DataSourceType dataSourceType, int dataSourceId, 
			DestinationLocationType destinationLocationType, int destinationLocationId)
		{
			DataSourceType = dataSourceType;
			DataSourceId = dataSourceId;
			DestinationLocationType = destinationLocationType;
			DestinationLocationId = destinationLocationId;
			CopyImagesMode = ImportImageFileCopyMode.SetFileLinks;
		}
	}
}
