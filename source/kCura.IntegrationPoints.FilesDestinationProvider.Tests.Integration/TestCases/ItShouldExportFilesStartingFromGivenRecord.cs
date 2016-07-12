using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportFilesStartingFromGivenRecord : ExportTestCaseBase
	{
		private readonly int _startExportAtRecord = 2;

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.CopyFileFromRepository = true;
			settings.IncludeNativeFilesPath = true;
			settings.ExportImages = false;

			settings.StartExportAtRecord = _startExportAtRecord;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories).ToList();

			var actualFileCount = Directory.EnumerateFiles(nativesRootDirectory[0].FullName, "*", SearchOption.AllDirectories).Count();

			var expectedFileCount = documents.Rows.Count - (_startExportAtRecord - 1);
			Assert.AreEqual(expectedFileCount, actualFileCount);
		}
	}
}