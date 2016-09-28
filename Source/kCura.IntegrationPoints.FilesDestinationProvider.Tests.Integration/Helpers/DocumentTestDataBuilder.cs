﻿using System.Collections.Generic;
using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	internal class DocumentTestDataBuilder
	{
		public static DocumentsTestData BuildTestData()
		{
			var foldersWithDocuments = GetFoldersWithDocuments();
			var images = GetImageDataTable();
			return new DocumentsTestData(foldersWithDocuments, images);
		}

		private static IEnumerable<FolderWithDocuments> GetFoldersWithDocuments()
		{
			var firstFolder = new FolderWithDocuments("first", CreateDataTableForDocuments());
			firstFolder.Documents.Rows.Add("AMEYERS_0000757", "AMEYERS_0000757.htm",
				Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\NATIVES\AMEYERS_0000757.htm"), "Level1\\Level2", true);

			var firstFolderChild = new FolderWithDocuments("child", CreateDataTableForDocuments());
			firstFolderChild.Documents.Rows.Add("AMEYERS_0000975", "AMEYERS_0000975.pdf",
				Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\NATIVES\AMEYERS_0000975.pdf"), "Level1\\Level2", true);

			firstFolderChild.ParentFolderWithDocuments = firstFolder;

			var secondFolder = new FolderWithDocuments("second", CreateDataTableForDocuments());
			secondFolder.Documents.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185.xls",
				Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\NATIVES\AMEYERS_0001185.xls"), "Level1\\Level2", true);
			secondFolder.Documents.Rows.Add("AZIPPER_0011318", "AZIPPER_0011318.msg",
				Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\NATIVES\AZIPPER_0011318.msg"), "Level1\\Level2", false);

			return new[] {firstFolder, firstFolderChild, secondFolder};
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

		private static DataTable GetImageDataTable()
		{
			var table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add("Control Number", typeof(string));
			table.Columns.Add("Bates Beg", typeof(string));
			table.Columns.Add("File", typeof(string));

			table.Rows.Add("AMEYERS_0000757", "AMEYERS_0000757", Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\IMAGES\AMEYERS_0000757.tif"));
			table.Rows.Add("AMEYERS_0000975", "AMEYERS_0000975", Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\IMAGES\AMEYERS_0000975.tif"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185", Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\IMAGES\AMEYERS_0001185.tif"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185_001", Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\IMAGES\AMEYERS_0001185_001.tif"));

			return table;
		}
	}
}