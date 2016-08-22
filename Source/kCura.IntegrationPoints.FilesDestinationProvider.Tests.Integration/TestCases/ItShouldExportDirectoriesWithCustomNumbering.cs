using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportDirectoriesWithCustomNumbering : ExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.CopyFileFromRepository = true;
			settings.ExportImages = true;
			settings.IncludeNativeFilesPath = true;

			settings.SubdirectoryMaxFiles = 1;
			settings.SubdirectoryStartNumber = 3;
			settings.SubdirectoryDigitPadding = 5;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var fileCount = documents.Rows.Count;
			ValidateDirectoriesExistence("NATIVES", directory, fileCount);
			ValidateDirectoriesExistence("IMAGES", directory, fileCount);
			//TODO this can be done after export extracted text is completed
			//ValidateDirectoriesExistence("TEXT", directory, images);
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