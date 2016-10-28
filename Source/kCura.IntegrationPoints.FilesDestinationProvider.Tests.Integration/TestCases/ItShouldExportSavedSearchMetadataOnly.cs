using System.IO;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportSavedSearchMetadataOnly : MetadataExportTestCaseBase
	{
		public override string MetadataFormat => "dat";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.None;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			// verify that metadata file was created
			var actual = GetFileInfo(directory);

			var expectedMetadataFilename = $"{ExportSettings.SavedSearchName}_export.{MetadataFormat}";

			Assert.That(actual, Is.Not.Null);
			Assert.That(actual?.Name, Is.EqualTo(expectedMetadataFilename));
			Assert.That(actual?.Length, Is.GreaterThan(0));

			// verify that no other files were exported
			var numberOfOtherFiles = directory.EnumerateFiles("*", SearchOption.AllDirectories)
				.Count(f => !f.Name.Equals(expectedMetadataFilename));

			Assert.That(numberOfOtherFiles, Is.EqualTo(0));
		}
	}
}