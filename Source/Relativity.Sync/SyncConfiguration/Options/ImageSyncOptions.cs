using Relativity.Sync.Configuration;

namespace Relativity.Sync.SyncConfiguration.Options
{
	/// <summary>
	/// 
	/// </summary>
	public class ImageSyncOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public ImportImageFileCopyMode CopyImagesMode { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public DestinationLocationType DestinationLocationType { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int DestinationLocationId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public DataSourceType DataSourceType { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int DataSourceId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataSourceType"></param>
		/// <param name="dataSourceId"></param>
		/// <param name="destinationLocationType"></param>
		/// <param name="destinationLocationId"></param>
		/// <param name="copyImagesMode"></param>
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
