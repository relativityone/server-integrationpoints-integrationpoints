﻿using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportImageDataFileAsIPROFullText : MetadataExportTestCaseBase
	{
		public override string MetadataFormat => "lfp";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = true;
			settings.SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.IPRO_FullText;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var fileInfo = GetFileInfo(directory);
			Assert.That(fileInfo.Name, Is.EqualTo($"{ExportSettings.SavedSearchName}_export_FULLTEXT_.{MetadataFormat}"));
			Assert.That(DataFileFormatHelper.FileStartWith("FT,AMEYERS_0000757", fileInfo));
		}
	}
}