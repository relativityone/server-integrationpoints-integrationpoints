using Relativity.Sync.Configuration;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class DocumentSyncOptions
	{
		public int SavedSearchId { get; }

		public int DestinationFolderId { get; }

		public ImportNativeFileCopyMode CopyNativesMode { get; set; }

		public DocumentSyncOptions(int savedSearchId, int destinationFolderId)
		{
			SavedSearchId = savedSearchId;
			DestinationFolderId = destinationFolderId;
		}
	}
}
