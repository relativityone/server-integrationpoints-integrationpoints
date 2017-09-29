using System.IO;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Resources;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportImageDataFileAsOpticon : MetadataExportTestCaseBase
	{
		public override string MetadataFormat => "opt";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = true;
			settings.SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.Opticon;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var fileInfo = GetFileInfo(directory);
			Assert.That(fileInfo.Name, Is.EqualTo($"{ExportSettings.SavedSearchName}_export.{MetadataFormat}"));
			Assert.That(DataFileFormatHelper.FileStartWith("AMEYERS_0000757", fileInfo));
            Assert.AreEqual(ExpectedOutput.Opticon, DataFileFormatHelper.GetContent(fileInfo));


		}
	}
}