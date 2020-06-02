using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	public class ItShouldExportProducedImagesPrecedence : ExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportNatives = true;
			settings.ExportImages = true;
			settings.ProductionPrecedence = ExportSettings.ProductionPrecedenceType.Produced;
			settings.ImageType = ExportSettings.ImageFileType.SinglePage;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var imagesDirectory = directory.GetDirectories("IMAGES", SearchOption.AllDirectories).First();
			var exportedFiles = imagesDirectory.GetFiles("*.*", SearchOption.AllDirectories).OrderBy(x => x.Name).ToArray();
			var filesCount = exportedFiles.Length;

			var expectedFilesCount = documentsTestData.Images.Rows.Count;
			var expectdFiles = documentsTestData.Images.Rows.Cast<DataRow>()
				.Select(x => new FileInfo((string)x.ItemArray[2])).OrderBy(x => x.Name);


			Assert.AreEqual(filesCount, expectedFilesCount);
			Assert.True(exportedFiles.SequenceEqual(expectdFiles, new FileEqualityComparer()));
		}
	}
}