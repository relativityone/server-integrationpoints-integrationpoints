﻿using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportImageDataFileAsIPRO : MetadataExportTestCaseBase
    {
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = true;
			settings.SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.IPRO;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var fileInfo = GetFileInfo(directory);
            Assert.That(fileInfo.Name, Is.EqualTo($"{ExportSettings.SavedSearchName}_export.{MetadataFormat}"));
			Assert.That(DataFileFormatHelper.FileStartWith("IM,AMEYERS_0000757", fileInfo));
		}

		public override string MetadataFormat => "lfp";
	}
}
