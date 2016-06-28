using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportFilePathUserPrefixImage : MetadataExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = true;
			settings.CopyFileFromRepository = true;
			settings.SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.Opticon;
			settings.FilePath = ExportSettings.FilePathType.Prefix;
			settings.UserPrefix = "USER1";

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var fileInfo = GetFileInfo(directory);
			Assert.That(DataFileFormatHelper.LineNumberContains(1,
				@"USER1\0\IMAGES\1\AMEYERS_0000757.tif", fileInfo));
		}

		public override string MetadataFormat => "opt";
	}
}
