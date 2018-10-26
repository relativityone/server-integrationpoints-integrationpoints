using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace kCura.IntegrationPoint.Tests.Core.Models
{
	public class DocumentsTestData
	{
		private DataTable _allDocumentsDataTable;

		public DocumentsTestData(IList<FolderWithDocuments> documents, DataTable images, int? rootFolderId)
		{
			Documents = documents;
			Images = images;
			RootFolderId = rootFolderId;
		}

		public IList<FolderWithDocuments> Documents { get; }

		public DataTable AllDocumentsDataTable
		{
			get
			{
				if (_allDocumentsDataTable == null)
				{
					_allDocumentsDataTable = Documents.First().Documents.Clone();
					foreach (var folderWithDocuments in Documents)
					{
						foreach (DataRow documentsRow in folderWithDocuments.Documents.Rows)
						{
							_allDocumentsDataTable.ImportRow(documentsRow);
						}
					}
				}
				return _allDocumentsDataTable;
			}
		}

		public DataTable Images { get; }
		/// <summary>
		/// Set to null - workspace root folder id will be set during import and folder path will be taken from Folder Path column in DataTable
		/// Set to folder id - all documents will go there, not to folders
		/// </summary>
		public int? RootFolderId { get; }
	}
}