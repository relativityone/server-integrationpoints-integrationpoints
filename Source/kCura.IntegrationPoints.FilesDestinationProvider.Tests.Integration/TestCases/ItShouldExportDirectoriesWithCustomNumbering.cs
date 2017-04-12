using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportDirectoriesWithCustomNumbering : ExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportNatives = true;
			settings.ExportImages = true;
			settings.ExportFullTextAsFile = true;

			settings.ImageType = ExportSettings.ImageFileType.SinglePage;
			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;
			settings.DataFileEncoding = Encoding.UTF8;
			settings.TextFileEncodingType = Encoding.UTF8;

			settings.SubdirectoryMaxFiles = 1;
			settings.SubdirectoryStartNumber = 3;
			settings.SubdirectoryDigitPadding = 5;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var fileCount = documentsTestData.AllDocumentsDataTable.Rows.Count;
			ValidateDirectoriesExistence("NATIVES", directory, fileCount);
			ValidateDirectoriesExistence("IMAGES", directory, fileCount);
			ValidateDirectoriesExistence("TEXT", directory, fileCount);
		}

		private void ValidateDirectoriesExistence(string rootDirectoryName, DirectoryInfo directory, int fileCount)
		{
			var rootDirectory = directory.EnumerateDirectories(rootDirectoryName, SearchOption.AllDirectories).ToList();

			var dirCount = fileCount/ExportSettings.SubdirectoryMaxFiles;

			for (var i = 0; i < dirCount; i++)
			{
				var expectedFileName = (i + ExportSettings.SubdirectoryStartNumber).ToString().PadLeft(ExportSettings.SubdirectoryDigitPadding, '0');
				Assert.True(rootDirectory.Any(x => x.EnumerateDirectories().Any(y => y.Name == expectedFileName)));
			}
		}
	}
}