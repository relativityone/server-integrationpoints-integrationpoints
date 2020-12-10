using System.Collections.Generic;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.SyncConfiguration.Options
{
	/// <summary>
	/// 
	/// </summary>
	public class DocumentSyncOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public int SavedSearchId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int DestinationFolderId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public List<FieldMap> FieldsMapping { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public ImportNativeFileCopyMode CopyNativesMode { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="savedSearchId"></param>
		/// <param name="destinationFolderId"></param>
		/// <param name="fieldsMapping"></param>
		/// <param name="copyNativesMode"></param>
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
