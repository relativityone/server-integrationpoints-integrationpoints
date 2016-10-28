using System.IO;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldNotExportImageDataFileWhenExportImagesNotSelected : MetadataExportTestCaseBase
	{
		public override string MetadataFormat => "opt";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = false;
			settings.SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.None;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			Assert.Throws<FileNotFoundException>(() => GetFileInfo(directory));
		}
	}
}