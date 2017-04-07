using System;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	public class ItShouldExportFolderWithNatives : ExportTestCaseBase
	{
		private readonly ConfigSettings _configSettings;
		private FolderWithDocuments _folderWithDocuments;

		public ItShouldExportFolderWithNatives(ConfigSettings configSettings)
		{
			_configSettings = configSettings;
		}

		public override ExportSettings Prepare(ExportSettings settings)
		{
			_folderWithDocuments = FindFolderWithDocuments();

			settings.TypeOfExport = ExportSettings.ExportType.Folder;
			settings.ViewId = _configSettings.ViewId;
			settings.ViewName = _configSettings.ViewName;

			settings.FolderArtifactId = _folderWithDocuments.FolderId.Value;

			settings.ExportNatives = true;

			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;

			return base.Prepare(settings);
		}

		private FolderWithDocuments FindFolderWithDocuments()
		{
			foreach (var folderWithDocuments in _configSettings.DocumentsTestData.Documents)
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
			var nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories).ToList();
			var actualFileCount = Directory.EnumerateFiles(nativesRootDirectory[0].FullName, "*", SearchOption.AllDirectories).Count();

			Assert.That(actualFileCount, Is.EqualTo(_folderWithDocuments.Documents.Rows.Count));

			var dataFile = DataFileFormatHelper.GetFileInFormat("*.dat", directory);
			var rowsInDataFile = File.ReadAllLines(dataFile.FullName).Length;

			Assert.That(rowsInDataFile - 1, Is.EqualTo(_folderWithDocuments.Documents.Rows.Count));
		}
	}
}