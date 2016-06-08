using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.WinEDDS;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	public class ItShouldExportImageDataFileAsOpticon : BaseMetadataExportTestCase
    {
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = true;
			settings.SelectedImageDataFileFormat = LoadFileType.FileFormat.Opticon;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var fileInfo = GetFileInfo(directory);
            Assert.That(fileInfo.Name, Is.EqualTo($"{_exportSettings.ExportedObjName}_export.{MetadataFormat}"));
			Assert.That(DataFileFormatHelper.FileStartWith("AMEYERS_0000757", fileInfo));
		}
		public override string MetadataFormat => "opt";
	}
}
