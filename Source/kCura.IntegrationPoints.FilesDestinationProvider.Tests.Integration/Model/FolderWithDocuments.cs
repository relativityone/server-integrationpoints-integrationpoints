using System.Data;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model
{
	public class FolderWithDocuments
	{
		public FolderWithDocuments(string folderName, DataTable documents)
		{
			FolderName = folderName;
			Documents = documents;
		}

		public string FolderName { get; }

		public int? FolderId { get; set; }

		public DataTable Documents { get; }

		public FolderWithDocuments ParentFolderWithDocuments { get; set; }
	}
}