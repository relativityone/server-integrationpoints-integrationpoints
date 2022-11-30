using Relativity.Sync.Configuration;

namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Represents image synchronization options.
    /// </summary>
    public class ImageSyncOptions
    {
        /// <summary>
        /// Determines the destination location type.
        /// </summary>
        public DestinationLocationType DestinationLocationType { get; }

        /// <summary>
        /// Specifies destination location object Artifact ID.
        /// </summary>
        public int DestinationLocationId { get; }

        /// <summary>
        /// Determines data source type.
        /// </summary>
        public DataSourceType DataSourceType { get; }

        /// <summary>
        /// Specifies data source object Artifact ID.
        /// </summary>
        public int DataSourceId { get; }

        /// <summary>
        /// Determines import image file copy mode.
        /// </summary>
        public ImportImageFileCopyMode CopyImagesMode { get; set; }

        /// <summary>
        /// Specifies if transferred images should be tagged
        /// </summary>
        public bool EnableTagging { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="ImageSyncOptions"/> class.
        /// </summary>
        /// <param name="dataSourceType">Data source type.</param>
        /// <param name="dataSourceId">Data source object Artifact ID.</param>
        /// <param name="destinationLocationType">Destination location type.</param>
        /// <param name="destinationLocationId">Destination object Artifact ID.</param>
        public ImageSyncOptions(
            DataSourceType dataSourceType,
            int dataSourceId,
            DestinationLocationType destinationLocationType,
            int destinationLocationId)
        {
            DataSourceType = dataSourceType;
            DataSourceId = dataSourceId;
            DestinationLocationType = destinationLocationType;
            DestinationLocationId = destinationLocationId;
            CopyImagesMode = ImportImageFileCopyMode.SetFileLinks;
        }
    }
}
