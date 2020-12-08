using System.Collections.Generic;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.SyncConfiguration.Options
{
	public class DocumentSyncOptions
	{
		public int SavedSearchId { get; set; }
		public int DestinationFolderId { get; set; }
		public List<FieldMap> FieldsMapping { get; set; }
		public ImportNativeFileCopyMode CopyNativesMode { get; set; }

		public DocumentSyncOptions(int savedSearchId, int destinationFolderId, List<FieldMap> fieldsMapping = null, 
			ImportNativeFileCopyMode copyNativesMode = ImportNativeFileCopyMode.DoNotImportNativeFiles)
		{
			SavedSearchId = savedSearchId;
			DestinationFolderId = destinationFolderId;
			FieldsMapping = fieldsMapping ?? new List<FieldMap>();
			CopyNativesMode = copyNativesMode;
		}
	}
}
