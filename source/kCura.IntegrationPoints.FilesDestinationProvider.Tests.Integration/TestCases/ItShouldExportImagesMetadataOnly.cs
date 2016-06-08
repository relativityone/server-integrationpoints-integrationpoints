﻿using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportImagesMetadataOnly : BaseMetadataExportTestCase
    {
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = true;
			settings.CopyFileFromRepository = false;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			// verify that metadata file was created
			var actual = GetFileInfo(directory);
            Assert.That(actual?.Name, Is.EqualTo($"{ExportSettings.ExportedObjName}_export.{MetadataFormat}"));
			Assert.That(actual?.Length, Is.Positive);

			// verify that no images were exported
			var unwantedFileExtensions = new[] { ".tif", ".tiff", ".jpg", ".jpeg", ".pdf" };

			var numberOfImages = directory.EnumerateFiles("*", SearchOption.AllDirectories)
				.Count(f => unwantedFileExtensions.Contains(f.Extension));

			Assert.That(numberOfImages, Is.EqualTo(0));
		}
		
		public override string MetadataFormat => "opt";
	}
}
