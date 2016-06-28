using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldNotExportImageDataFileWhenExportImagesNotSelected : MetadataExportTestCaseBase
    {
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = false;
			settings.SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.Opticon;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			Assert.Throws<FileNotFoundException>(() => GetFileInfo(directory));
		}
		public override string MetadataFormat => "opt";
	}
}
