using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportVolumeWithCustomName : ExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportNatives = true;
			settings.ExportImages = true;

			settings.VolumePrefix = "test_volume_prefix";
			settings.VolumeDigitPadding = 5;
			settings.VolumeStartNumber = 3;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			string expectedDirectoryName = $"{ExportSettings.VolumePrefix}00003";

			var volumeDirectories = directory.EnumerateDirectories(expectedDirectoryName, SearchOption.TopDirectoryOnly).ToList();

			Assert.That(volumeDirectories.Any(), $"There should be volume folder named {expectedDirectoryName}");
		}
	}
}