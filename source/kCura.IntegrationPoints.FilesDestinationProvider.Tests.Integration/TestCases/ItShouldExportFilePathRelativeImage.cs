using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	public class ItShouldExportFilePathRelativeImage : MetadataExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = true;
			settings.CopyFileFromRepository = true;
			settings.SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.Opticon;
			settings.FilePath = ExportSettings.FilePathType.Relative;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var fileInfo = GetFileInfo(directory);
			Assert.That(DataFileFormatHelper.LineNumberContains(1, @".\VOL00000001\IMAGES\00000001\AMEYERS_0000757.tif", fileInfo));
		}

		public override string MetadataFormat => "opt";
	}
}
