using System.IO;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportImagesMetadataOnly : MetadataExportTestCaseBase
	{
		public override string MetadataFormat => "opt";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = false;
			settings.ExportNatives = false;
			settings.SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.Opticon;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			// verify that metadata file was created
			var actual = GetFileInfo(directory);
			Assert.That(actual?.Name, Is.EqualTo($"{ExportSettings.SavedSearchName}_export.{MetadataFormat}"));
			Assert.That(actual?.Length, Is.GreaterThan(0));

			// verify that no images were exported
			var unwantedFileExtensions = new[] {".tif", ".tiff", ".jpg", ".jpeg", ".pdf"};

			var numberOfImages = directory.EnumerateFiles("*", SearchOption.AllDirectories)
				.Count(f => unwantedFileExtensions.Contains(f.Extension));

			Assert.That(numberOfImages, Is.EqualTo(0));
		}
	}
}