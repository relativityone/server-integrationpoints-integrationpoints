using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
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

        public static DocumentsTestData BuildTestData(
            string prefix = "",
            string testDirectory = null,
            bool withNatives = true,
            TestDataType testDataType = TestDataType.SmallWithFoldersStructure)
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
                    foldersWithDocuments = GetFoldersWithDocumentsBasedOnDirectoryStructureOfNatives(Path.Combine(testDirectory, TestDataNativesPath), withNatives);
                    images = GetImageDataTableForAllNativesInGivenFolder(prefix, testDirectory, TestDataPath);
                    break;
                case TestDataType.SmallWithoutFolderStructure:
                    foldersWithDocuments = GetDocumentsIntoRootFolder(prefix, Path.Combine(testDirectory, TestDataNativesPath), withNatives);
                    images = GetImageDataTableForAllNativesInGivenFolder(prefix, testDirectory, TestDataPath);
                    break;
                case TestDataType.ModerateWithFoldersStructure:
                    foldersWithDocuments = GetFoldersWithDocumentsBasedOnDirectoryStructureOfNatives(Path.Combine(testDirectory, TestDataExtendedNativesPath), withNatives);
                    images = GetImageDataTableForAllNativesInGivenFolder(prefix, testDirectory, TestDataExtendedPath);
                    break;
                case TestDataType.ModerateWithoutFoldersStructure:
                    foldersWithDocuments = GetDocumentsIntoRootFolder(prefix, Path.Combine(testDirectory, TestDataExtendedNativesPath), withNatives);
                    images = GetImageDataTableForAllNativesInGivenFolder(prefix, testDirectory, TestDataExtendedPath);
                    break;
                case TestDataType.TextWithoutFolderStructure:
                    foldersWithDocuments = GetDocumentsIntoRootFolder(prefix, Path.Combine(testDirectory, TestDataTextNativesPath), withNatives);
                    images = GetImageDataTableForAllNativesInGivenFolder(prefix, testDirectory, TestDataTextPath);
                    break;
                case TestDataType.SaltPepperWithFolderStructure:
                    foldersWithDocuments = GetDocumentsIntoRootFolder(prefix, Path.Combine(testDirectory, SaltPepperTestDataNativesPath), withNatives);
                    images = GetImageDataTableForAllNativesInGivenFolder(prefix, testDirectory, SaltPepperTestDataPath);
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

            // The document identifier column name must match the field name in the workspace.
            table.Columns.Add(TestConstants.FieldNames.CONTROL_NUMBER, typeof(string));
            table.Columns.Add(TestConstants.FieldNames.FILE_NAME, typeof(string));
            table.Columns.Add(TestConstants.FieldNames.NATIVE_FILE, typeof(string));
            table.Columns.Add(TestConstants.FieldNames.ISSUE_DESIGNATION, typeof(string));
            table.Columns.Add(TestConstants.FieldNames.HAS_IMAGES, typeof(bool));
            table.Columns.Add(TestConstants.FieldNames.FOLDER_PATH, typeof(string));
            table.Columns.Add(TestConstants.FieldNames.DOCUMENT_EXTENSION, typeof(string));
            return table;
        }

        private static string GetFoldersPrefix(string folderPath) =>
            folderPath.Replace(Path.GetFileName(folderPath), "");
        private static string GetDocumentFolderRelativePath(string folderPath, string foldersPrefix) =>
            folderPath.Replace(foldersPrefix, "").Replace("\\\\", "\\");
        private static IList<FolderWithDocuments> GetFoldersWithDocumentsBasedOnDirectoryStructureOfNatives(string nativesFolderPath, bool withNatives = true)
        {
            var foldersList = new List<FolderWithDocuments>();
            string[] folders = Directory.GetDirectories(nativesFolderPath, "*", SearchOption.AllDirectories);
            string foldersPrefix = GetFoldersPrefix(folders.First());

            foreach (string folderPath in folders)
            {
                var newFolder = new FolderWithDocuments(Path.GetFileName(folderPath), CreateDataTableForDocuments());

                FillFolderWithFiles(newFolder, folderPath, foldersPrefix, withNatives);

                // Link "Child folder" with "Parent folder"
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

        private static void FillFolderWithFiles(FolderWithDocuments newFolder, string folderPath, string foldersPrefix, bool withNatives)
        {
            const string issueDesignation = "Level1\\Level2";
            const bool hasImages = false;

            string documentsFolderPath = GetDocumentFolderRelativePath(folderPath, foldersPrefix);
            foreach (string filePath in Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly))
            {
                newFolder.Documents.Rows.Add(
                    Path.GetFileNameWithoutExtension(filePath),
                    Path.GetFileName(filePath),
                    withNatives ? filePath : string.Empty,
                    issueDesignation,
                    hasImages,
                    documentsFolderPath,
                    Path.GetExtension(filePath)?.ToUpperInvariant() ?? string.Empty);
            }
        }

        private static IList<FolderWithDocuments> GetDocumentsIntoRootFolder(string prefix, string nativesFolderPath, bool withNatives = true)
        {
            const string issueDesignation = "Level1\\Level2";
            const bool hasImages = false;

            var foldersList = new List<FolderWithDocuments>();

            var newFolder = new FolderWithDocuments(Path.GetFileName(nativesFolderPath), CreateDataTableForDocuments());

            foreach (string filePath in Directory.GetFiles(nativesFolderPath, "*", SearchOption.AllDirectories))
            {
                newFolder.Documents.Rows.Add(
                    prefix + Path.GetFileNameWithoutExtension(filePath),
                    Path.GetFileName(filePath),
                    withNatives ? filePath : string.Empty,
                    issueDesignation,
                    hasImages,
                    newFolder.FolderName,
                    Path.GetExtension(filePath)?.ToUpperInvariant() ?? string.Empty);
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
            table.Columns.Add(TestConstants.FieldNames.CONTROL_NUMBER, typeof(string));
            table.Columns.Add(TestConstants.FieldNames.BATES_BEG, typeof(string));
            table.Columns.Add(TestConstants.FieldNames.FILE, typeof(string));
            return table;
        }

        private static DataTable GetImageDataTableForAllNativesInGivenFolder(string prefix, string testDirectory, string testDataDirectory)
        {
            string nativesFolderPath = Path.Combine(testDirectory, testDataDirectory, "NATIVES");
            string imagesFolderPath = Path.Combine(testDirectory, testDataDirectory, "IMAGES");
            DataTable tableOfImages = CreateDataTableForImages();

            foreach (string nativeFileName in Directory.GetFiles(nativesFolderPath, "*", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension))
            {
                foreach (string imageFilePath in GetListOfImagesForGivenNativeFile(nativeFileName, imagesFolderPath))
                {
                    tableOfImages.Rows.Add(CreateImageRowBasedOnImageFilePath(prefix, nativeFileName, imageFilePath));
                }
            }

            return tableOfImages;
        }

        private static IEnumerable<string> GetListOfImagesForGivenNativeFile(string nativeFileName, string imagesFolderPath)
        {
            return Directory.GetFiles(imagesFolderPath, $"{nativeFileName}*.tif", SearchOption.AllDirectories);
        }

        private static object[] CreateImageRowBasedOnImageFilePath(string prefix, string nativeFileName, string imageFilePath)
        {
            return new object[]
            {
                prefix + nativeFileName,
                prefix + Path.GetFileNameWithoutExtension(imageFilePath),
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
