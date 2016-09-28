using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model
{
	public class DocumentsTestData
	{
		private DataTable _allDocumentsDataTable;

		public DocumentsTestData(IEnumerable<FolderWithDocuments> documents, DataTable images)
		{
			Documents = documents;
			Images = images;
		}

		public IEnumerable<FolderWithDocuments> Documents { get; }

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
	}
}