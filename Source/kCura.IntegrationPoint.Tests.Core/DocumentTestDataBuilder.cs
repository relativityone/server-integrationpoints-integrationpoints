using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class DocumentTestDataBuilder
	{
		private static readonly string TestDataPath = @"TestData";
		private static readonly string TestDataNativesPath = $@"{TestDataPath}\NATIVES";
		private static readonly string TestDataExtendedPath = @"TestDataExtended";
		private static readonly string TestDataExtendedNativesPath = $@"{TestDataExtendedPath}\NATIVES";
		private static readonly string TestDataTextPath = @"TestDataText";
		private static readonly string TestDataTextNativesPath = $@"{TestDataTextPath}\NATIVES";
		private static readonly string SaltPepperTestDataPath = @"TestDataSaltPepper";
		private static readonly string SaltPepperTestDataNativesPath = $@"{SaltPepperTestDataPath}\NATIVES";

		public static DocumentsTestData BuildTestData(string testDirectory = null, bool withNatives = true, TestDataType testDataType = TestDataType.SmallWithFoldersStructure)
		{
			if (string.IsNullOrEmpty(testDirectory))
			{
				testDirectory = TestContext.CurrentContext.TestDirectory;
			}

			IList<FolderWithDocuments> foldersWithDocuments;
			DataTable images;

			switch (testDataType)
			{
				case TestDataType.SmallWithFoldersStructure:
					foldersWithDocuments = GetFoldersWithDocuments(testDirectory, withNatives);
					images = GetImageDataTable(testDirectory);
					break;
				case TestDataType.SmallWithoutFolderStructure:
					foldersWithDocuments = GetDocumentsIntoRootFolder(Path.Combine(testDirectory, TestDataNativesPath), withNatives);
					images = GetImageDataTableForAllNativesInGivenFolder(testDirectory, TestDataExtendedPath);
					break;
				case TestDataType.ModerateWithFoldersStructure:
					foldersWithDocuments = GetFoldersWithDocumentsBasedOnDirectoryStructureOfNatives(Path.Combine(testDirectory, TestDataExtendedNativesPath), withNatives);
					images = GetImageDataTableForAllNativesInGivenFolder(testDirectory, TestDataExtendedPath);
					break;
				case TestDataType.ModerateWithoutFoldersStructure:
					foldersWithDocuments = GetDocumentsIntoRootFolder(Path.Combine(testDirectory, TestDataExtendedNativesPath), withNatives);
					images = GetImageDataTableForAllNativesInGivenFolder(testDirectory, TestDataExtendedPath);
					break;
				case TestDataType.TextWithoutFolderStructure:
					foldersWithDocuments = GetDocumentsIntoRootFolder(Path.Combine(testDirectory, TestDataTextNativesPath), withNatives);
					images = GetImageDataTableForAllNativesInGivenFolder(testDirectory, TestDataTextPath);
					break;
				case TestDataType.SaltPepperWithFolderStructure:
					foldersWithDocuments = GetDocumentsIntoRootFolder(Path.Combine(testDirectory, SaltPepperTestDataNativesPath), withNatives);
					images = GetImageDataTableForAllNativesInGivenFolder(testDirectory, SaltPepperTestDataPath);
					break;
				default:
					throw new Exception("Unsupported TestDataType parameter");
			}
			return new DocumentsTestData(foldersWithDocuments, images);
		}

		#region Documents
		private static DataTable CreateDataTableForDocuments()
		{
			var table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add(Constants.CONTROL_NUMBER_FIELD, typeof(string));
			table.Columns.Add(Constants.FILE_NAME_FIELD, typeof(string));
			table.Columns.Add(Constants.NATIVE_FILE_FIELD, typeof(string));
			table.Columns.Add(Constants.ISSUE_DESIGNATION_FIELD, typeof(string));
			table.Columns.Add(Constants.HAS_IMAGES_FIELD, typeof(bool));
			return table;
		}

		private static IList<FolderWithDocuments> GetFoldersWithDocuments(string testDirectory, bool withNatives = true)
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

		private static IList<FolderWithDocuments> GetFoldersWithDocumentsBasedOnDirectoryStructureOfNatives(string nativesFolderPath, bool withNatives = true)
		{
			var foldersList = new List<FolderWithDocuments>();

			foreach (string folderPath in Directory.GetDirectories(nativesFolderPath, "*", SearchOption.AllDirectories))
			{
				var newFolder = new FolderWithDocuments(Path.GetFileName(folderPath), CreateDataTableForDocuments());

				FillFolderWithFiles(newFolder, folderPath, withNatives);

				//Link "Child folder" with "Parent folder"
				DirectoryInfo parentFolderInfo = Directory.GetParent(folderPath);
				if (parentFolderInfo.FullName != nativesFolderPath && foldersList.Any(n => n.FolderName == parentFolderInfo.Name))
				{
					FolderWithDocuments parentFolder = foldersList.FirstOrDefault(n => n.FolderName == parentFolderInfo.Name);

					newFolder.ParentFolderWithDocuments = parentFolder;
					parentFolder?.ChildrenFoldersWithDocument.Add(newFolder);
				}

				foldersList.Add(newFolder);
			}

			return foldersList;
		}

		private static void FillFolderWithFiles(FolderWithDocuments newFolder, string folderPath, bool withNatives)
		{
			foreach (string filePath in Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly))
			{
				newFolder.Documents.Rows.Add(Path.GetFileNameWithoutExtension(filePath), Path.GetFileName(filePath), withNatives ? filePath : string.Empty, "Level1\\Level2", false);
			}
		}

		private static IList<FolderWithDocuments> GetDocumentsIntoRootFolder(string nativesFolderPath, bool withNatives = true)
		{
			var foldersList = new List<FolderWithDocuments>();

			var newFolder = new FolderWithDocuments(Path.GetFileName(nativesFolderPath), CreateDataTableForDocuments());

			foreach (string filePath in Directory.GetFiles(nativesFolderPath, "*", SearchOption.AllDirectories))
			{
				newFolder.Documents.Rows.Add(Path.GetFileNameWithoutExtension(filePath), Path.GetFileName(filePath), withNatives ? filePath : string.Empty, "Level1\\Level2", false);
			}

			foldersList.Add(newFolder);

			return foldersList;
		}
		#endregion

		#region Images

		private static DataTable CreateDataTableForImages()
		{
			var table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add(Constants.CONTROL_NUMBER_FIELD, typeof(string));
			table.Columns.Add(Constants.BATES_BEG_FIELD, typeof(string));
			table.Columns.Add(Constants.FILE_FIELD, typeof(string));
			return table;
		}

		private static DataTable GetImageDataTable(string testDirectory)
		{
			DataTable table = CreateDataTableForImages();

			table.Rows.Add("AMEYERS_0000757", "AMEYERS_0000757", Path.Combine(testDirectory, @"TestData\IMAGES\AMEYERS_0000757.tif"));
			table.Rows.Add("AMEYERS_0000975", "AMEYERS_0000975", Path.Combine(testDirectory, @"TestData\IMAGES\AMEYERS_0000975.tif"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185", Path.Combine(testDirectory, @"TestData\IMAGES\AMEYERS_0001185.tif"));
			table.Rows.Add("AMEYERS_0001185", "AMEYERS_0001185_001", Path.Combine(testDirectory, @"TestData\IMAGES\AMEYERS_0001185_001.tif"));

			return table;
		}

		private static DataTable GetImageDataTableForAllNativesInGivenFolder(string testDirectory, string testDataDirectory)
		{
			string nativesFolderPath = Path.Combine(testDirectory, testDataDirectory, "NATIVES");
			string imagesFolderPath = Path.Combine(testDirectory, testDataDirectory, "IMAGES");
			DataTable tableOfImages = CreateDataTableForImages();

			foreach (string nativeFileName in Directory.GetFiles(nativesFolderPath, "*", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension))
			{
				foreach (string imageFilePath in GetListOfImagesForGivenNativeFile(nativeFileName, imagesFolderPath))
				{
					tableOfImages.Rows.Add(CreateImageRowBasedOnImageFilePath(nativeFileName, imageFilePath));
				}
			}

			return tableOfImages;
		}

		private static IEnumerable<string> GetListOfImagesForGivenNativeFile(string nativeFileName, string imagesFolderPath)
		{
			return Directory.GetFiles(imagesFolderPath, $"{nativeFileName}*.tif", SearchOption.AllDirectories);
		}

		private static object[] CreateImageRowBasedOnImageFilePath(string nativeFileName, string imageFilePath)
		{
			return new object[]
			{
				nativeFileName,
				Path.GetFileNameWithoutExtension(imageFilePath),
				imageFilePath
			};
		}

		public enum TestDataType
		{
			SmallWithFoldersStructure, SmallWithoutFolderStructure, ModerateWithFoldersStructure, ModerateWithoutFoldersStructure, TextWithoutFolderStructure, SaltPepperWithFolderStructure
		}

		#endregion
	}
}