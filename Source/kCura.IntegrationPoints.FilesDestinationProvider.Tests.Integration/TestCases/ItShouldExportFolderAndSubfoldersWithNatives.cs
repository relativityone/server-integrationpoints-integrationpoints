using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	public class ItShouldExportFolderAndSubfoldersWithNatives : ExportTestCaseBase
	{
		private FolderWithDocuments _folderWithDocuments;

		private readonly ExportTestConfiguration _testConfiguration;
		private readonly ExportTestContext _testContext;

		public ItShouldExportFolderAndSubfoldersWithNatives(ExportTestContext testContext, ExportTestConfiguration testConfiguration)
		{
			_testConfiguration = testConfiguration;
			_testContext = testContext;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			_folderWithDocuments = FindFolderWithDocumentsAndChildren();

			settings.TypeOfExport = ExportSettings.ExportType.FolderAndSubfolders;
			settings.ViewId = _testContext.ViewID;
			settings.ViewName = _testConfiguration.ViewName;

			settings.FolderArtifactId = _folderWithDocuments.FolderId.Value;

			settings.ExportNatives = true;

			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;

			return base.Prepare(settings);
		}

		private FolderWithDocuments FindFolderWithDocumentsAndChildren()
		{
			foreach (var folderWithDocuments in _testContext.DocumentsTestData.Documents)
			{
				if (folderWithDocuments.Documents.Rows.Count != 0 && folderWithDocuments.ChildrenFoldersWithDocument.Count != 0)
				{
					return folderWithDocuments;
				}
			}
			throw new ArgumentException("Folder with documents and children not found");
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			int expectedDocumentCount = CountDocumentsInFolderAndSubfolders(_folderWithDocuments);

			List<DirectoryInfo> nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories).ToList();
			int actualFileCount = Directory.EnumerateFiles(nativesRootDirectory[0].FullName, "*", SearchOption.AllDirectories).Count();

			Assert.That(actualFileCount, Is.EqualTo(expectedDocumentCount));

			FileInfo dataFile = DataFileFormatHelper.GetFileInFormat("*.dat", directory);
			int rowsInDataFile = File.ReadAllLines(dataFile.FullName).Length;

			Assert.That(rowsInDataFile - 1, Is.EqualTo(expectedDocumentCount));
		}

		private int CountDocumentsInFolderAndSubfolders(FolderWithDocuments currentFolder)
		{
			return currentFolder.Documents.Rows.Count + currentFolder.ChildrenFoldersWithDocument.Sum(CountDocumentsInFolderAndSubfolders);
		}
	}
}