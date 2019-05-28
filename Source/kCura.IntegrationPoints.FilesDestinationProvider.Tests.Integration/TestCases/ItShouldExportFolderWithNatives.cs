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
	public class ItShouldExportFolderWithNatives : ExportTestCaseBase
	{
		private FolderWithDocuments _folderWithDocuments;
		
		private readonly ExportTestConfiguration _testConfiguration;
		private readonly ExportTestContext _testContext;

		public ItShouldExportFolderWithNatives(ExportTestContext testContext, ExportTestConfiguration testConfiguration)
		{
			_testConfiguration = testConfiguration;
			_testContext = testContext;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			_folderWithDocuments = FindFolderWithDocuments();

			settings.TypeOfExport = ExportSettings.ExportType.Folder;
			settings.ViewId = _testContext.ViewID;
			settings.ViewName = _testConfiguration.ViewName;

			settings.FolderArtifactId = _folderWithDocuments.FolderId.Value;

			settings.ExportNatives = true;

			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;

			return base.Prepare(settings);
		}

		private FolderWithDocuments FindFolderWithDocuments()
		{
			foreach (FolderWithDocuments folderWithDocuments in _testContext.DocumentsTestData.Documents)
			{
				if (folderWithDocuments.Documents.Rows.Count != 0)
				{
					return folderWithDocuments;
				}
			}
			throw new ArgumentException("All folders are empty");
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			List<DirectoryInfo> nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories).ToList();
			int actualFileCount = Directory.EnumerateFiles(nativesRootDirectory[0].FullName, "*", SearchOption.AllDirectories).Count();

			Assert.That(actualFileCount, Is.EqualTo(_folderWithDocuments.Documents.Rows.Count));

			FileInfo dataFile = DataFileFormatHelper.GetFileInFormat("*.dat", directory);
			int rowsInDataFile = File.ReadAllLines(dataFile.FullName).Length;

			Assert.That(rowsInDataFile - 1, Is.EqualTo(_folderWithDocuments.Documents.Rows.Count));
		}
	}
}