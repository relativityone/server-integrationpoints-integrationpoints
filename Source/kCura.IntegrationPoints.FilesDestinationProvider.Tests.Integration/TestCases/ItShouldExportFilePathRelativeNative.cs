﻿using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportFilePathRelativeNative : MetadataExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance;
			settings.ExportNatives = true;
			settings.FilePath = ExportSettings.FilePathType.Relative;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var fileInfo = GetFileInfo(directory);
			Assert.That(DataFileFormatHelper.LineNumberContains(2, @"þ.\0\NATIVES\1\AMEYERS_0000757.htmþ", fileInfo));
		}

		public override string MetadataFormat => "dat";
	}
}