using System.Collections.Generic;
using System.Data;
using System.IO;
using kCura.IntegrationPoint.Tests.Core.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class DocumentTestDataBuilder
	{

		public static DocumentsTestData BuildTestData(string testDirectory = null, bool withNatives = true)
		{
			IList<FolderWithDocuments> foldersWithDocuments = GetFoldersWithDocuments(testDirectory ?? TestContext.CurrentContext.TestDirectory, withNatives);
			DataTable images = GetImageDataTable(testDirectory ?? TestContext.CurrentContext.TestDirectory);
			return new DocumentsTestData(foldersWithDocuments, images);
		}

		private static IList<FolderWithDocuments> GetFoldersWithDocuments(string testDirectory, bool withNatives)
		{
			var firstFolder = new FolderWithDocuments("first", CreateDataTableForDocuments());
			firstFolder.Documents.Rows.Add("AMEYERS_0000757", "AMEYERS_0000757.htm",
				withNatives ? Path.Combine(testDirectory, @"TestData\NATIVES\AMEYERS_0000757.htm") : string.Empty, "Level1\\Level2", true);

			var firstFolderChild = new FolderWithDocuments("child", CreateDataTableForDocuments());
			firstFolderChild.Documents.Rows.Add("AMEYERS_0000975", "AMEYERS_0000975.pdf",
				withNatives ? Path.Combine(testDirectory, @"TestData\NATIVES\AMEYERS_0000975.pdf") : string.Empty, "Level1\\Level2", true);

			firstFolderChild.ParentFolderWithDocuments = firstFolder;
			firstFolder.ChildrenFoldersWithDocument.Add(firstFolderChild);

			var secondFolder = new FolderWithDocuments("second", CreateDataTableForDocuments());
			secondFolder.Documents.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185.xls",
				withNatives ? Path.Combine(testDirectory, @"TestData\NATIVES\AMEYERS_0001185.xls") : string.Empty, "Level1\\Level2", true);
			secondFolder.Documents.Rows.Add("AZIPPER_0011318", "AZIPPER_0011318.msg",
				withNatives ? Path.Combine(testDirectory, @"TestData\NATIVES\AZIPPER_0011318.msg") : string.Empty, "Level1\\Level2", false);

			return new[] { firstFolder, firstFolderChild, secondFolder };
		}

		private static DataTable CreateDataTableForDocuments()
		{
			var table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add("Control Number", typeof(string));
			table.Columns.Add("File Name", typeof(string));
			table.Columns.Add("Native File", typeof(string));
			table.Columns.Add("Issue Designation", typeof(string));
			table.Columns.Add("Has Images", typeof(bool));
			return table;
		}

		private static DataTable GetImageDataTable(string testDirectory)
		{
			var table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add("Control Number", typeof(string));
			table.Columns.Add("Bates Beg", typeof(string));
			table.Columns.Add("File", typeof(string));

			table.Rows.Add("AMEYERS_0000757", "AMEYERS_0000757", Path.Combine(testDirectory, @"TestData\IMAGES\AMEYERS_0000757.tif"));
			table.Rows.Add("AMEYERS_0000975", "AMEYERS_0000975", Path.Combine(testDirectory, @"TestData\IMAGES\AMEYERS_0000975.tif"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185", Path.Combine(testDirectory, @"TestData\IMAGES\AMEYERS_0001185.tif"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185_001", Path.Combine(testDirectory, @"TestData\IMAGES\AMEYERS_0001185_001.tif"));

			return table;
		}
	}
}