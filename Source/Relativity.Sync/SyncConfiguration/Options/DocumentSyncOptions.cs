using Relativity.Sync.Configuration;

namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Represents document synchronization options.
    /// </summary>
    public class DocumentSyncOptions
    {
        /// <summary>
        /// Gets saved search Artifact ID.
        /// </summary>
        public int SavedSearchId { get; }

        /// <summary>
        /// Gets destination folder Artifact ID.
        /// </summary>
        public int DestinationFolderId { get; }

        /// <summary>
        /// Specifies copy native files mode.
        /// </summary>
        public ImportNativeFileCopyMode CopyNativesMode { get; set; }

        /// <summary>
        /// Specifies if transferred documents should be tagged
        /// </summary>
        public bool EnableTagging { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="DocumentSyncOptions"/> class.
        /// </summary>
        /// <param name="savedSearchId">Saved search Artifact ID.</param>
        /// <param name="destinationFolderId">Destination folder Artifact ID.</param>
        public DocumentSyncOptions(int savedSearchId, int destinationFolderId)
        {
            SavedSearchId = savedSearchId;
            DestinationFolderId = destinationFolderId;
        }
    }
}
