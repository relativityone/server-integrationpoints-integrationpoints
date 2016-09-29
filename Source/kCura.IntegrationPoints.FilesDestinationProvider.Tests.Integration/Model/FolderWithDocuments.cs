using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model
{
	public class FolderWithDocuments
	{
		public FolderWithDocuments(string folderName, DataTable documents)
		{
			FolderName = folderName;
			Documents = documents;
			ChildrenFoldersWithDocument = new List<FolderWithDocuments>();
		}

		public string FolderName { get; }

		public int? FolderId { get; set; }

		public DataTable Documents { get; }

		public FolderWithDocuments ParentFolderWithDocuments { get; set; }

		public IList<FolderWithDocuments> ChildrenFoldersWithDocument { get; }
	}
}