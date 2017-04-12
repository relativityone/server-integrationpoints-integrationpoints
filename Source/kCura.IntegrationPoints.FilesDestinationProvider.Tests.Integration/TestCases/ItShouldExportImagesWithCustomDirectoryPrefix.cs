using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportImagesWithCustomDirectoryPrefix : ExportTestCaseBase
	{
		private const string _PREFIX = "custom_image_prefix";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportNatives = true;
			settings.ExportImages = true;
			settings.SubdirectoryImagePrefix = _PREFIX;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var imagesRootDirectory = directory.EnumerateDirectories("IMAGES", SearchOption.AllDirectories).ToList();

			var expectedDirectories = imagesRootDirectory.SelectMany(x => x.EnumerateDirectories().Where(y => y.Name.StartsWith(_PREFIX)));

			Assert.That(expectedDirectories.Any(), $"There should be at least one folder with specified prefix {_PREFIX}");
		}
	}
}