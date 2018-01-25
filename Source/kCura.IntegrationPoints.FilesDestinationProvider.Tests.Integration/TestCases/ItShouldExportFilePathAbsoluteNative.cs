﻿using System.IO;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportFilePathAbsoluteNative : MetadataExportTestCaseBase
	{
		public override string MetadataFormat => "dat";

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;
			settings.ExportNatives = true;
			settings.FilePath = ExportSettings.FilePathType.Absolute;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			FileInfo fileInfo = GetFileInfo(directory);
			Assert.That(DataFileFormatHelper.LineNumberContains(2, $"þ{ExportSettings.ExportFilesLocation}\\{ExportSettings.VolumeStartNumber}\\NATIVES\\1\\AMEYERS_0000757.htmþ", fileInfo));
		}
	}
}