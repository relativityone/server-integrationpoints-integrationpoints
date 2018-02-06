using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	public class ItShouldExportOriginalImagesPrecendence : ExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportNatives = true;
			settings.ExportImages = true;
			settings.ProductionPrecedence = ExportSettings.ProductionPrecedenceType.Original;
			settings.ImageType = ExportSettings.ImageFileType.SinglePage;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var imagesDirectory = directory.GetDirectories("IMAGES", SearchOption.AllDirectories).First();
			var exportedFiles = imagesDirectory.GetFiles("*.*", SearchOption.AllDirectories);
			var filesCount = exportedFiles.Length;
			var expectedFilesCount = documentsTestData.Images.Rows.Count;

			Assert.AreEqual(filesCount, expectedFilesCount);

			for (int i = 0; i < exportedFiles.Length; i++)
			{
				var file1 = exportedFiles[i];
				var file2 = new FileInfo((string)documentsTestData.Images.Rows[i].ItemArray[2]);

				Assert.IsTrue(FileComparer.Compare(file1, file2));
			}
		}
	}
}