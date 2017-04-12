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
	public class ItShouldIncludeNativeFilesPathWithoutExportingNatives : ExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportNatives = false;
			settings.IncludeNativeFilesPath = true;
			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var nativesRootDirectory = directory.EnumerateDirectories("NATIVES", SearchOption.AllDirectories).ToList();

			Assert.False(nativesRootDirectory.Any());

			var datFile = DataFileFormatHelper.GetFileInFormat("*.dat", directory);

			Assert.That(DataFileFormatHelper.LineNumberContains(0, "þFILE_PATHþ", datFile));
		}
	}
}