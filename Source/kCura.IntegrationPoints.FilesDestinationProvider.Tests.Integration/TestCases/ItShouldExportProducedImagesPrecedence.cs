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
		private const string _PRODUCTION_BATES_PREFIX = "PRE";
		private const string _PRODUCTION_BATES_SUFFIX = "SUF";

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
			DirectoryInfo imagesDirectory = directory.GetDirectories("IMAGES", SearchOption.AllDirectories).First();
			FileInfo[] allExportedImages = imagesDirectory.GetFiles("*.*", SearchOption.AllDirectories);
			int totalExportedImagesCount = allExportedImages.Length;

			int expectedFilesWithImagesCount = documentsTestData.Images.Rows.Count;

			DataRow[] documentsWithImages = documentsTestData.AllDocumentsDataTable.Select($"[Has Images] = '{true}'");
			DataRow[] documentsWithoutImages = documentsTestData.AllDocumentsDataTable.Select($"[Has Images] = '{false}'");
			int numberofDocumentsWithoutImages = documentsWithoutImages.Length;

			Assert.AreEqual(totalExportedImagesCount, numberofDocumentsWithoutImages + expectedFilesWithImagesCount);

			AssertExportedImagesAreSameAsInputImages(documentsWithImages, documentsTestData.Images.Select(), allExportedImages);
			AssertExportedFilesWithoutImagesAreSameAsPlaceholder(documentsWithoutImages, allExportedImages);
		}

		private void AssertExportedFilesWithoutImagesAreSameAsPlaceholder(DataRow[] documentsWithoutImages,
			FileInfo[] exportedImages)
		{
			foreach (var document in documentsWithoutImages)
			{
				FileInfo file =
					exportedImages.SingleOrDefault(f => Path.GetFileNameWithoutExtension(f.Name) == GetProducedImageName(document.ItemArray[0].ToString()));

				Assert.IsNotNull(file);
				Assert.IsTrue(File.Exists(file.FullName));
			}
		}

		private static string GetProducedImageName(string controlNumber)
		{
			return $"{_PRODUCTION_BATES_PREFIX}{controlNumber}{_PRODUCTION_BATES_SUFFIX}";
		}

		private static void AssertExportedImagesAreSameAsInputImages(DataRow[] documentsWithoutImages, DataRow[] inputImages,
			FileInfo[] exportedImages)
		{
			foreach (var document in documentsWithoutImages)
			{
				IList<DataRow> imagesWithSameControlNumber =
					inputImages.Where(x => x.ItemArray[0].ToString() == document.ItemArray[0].ToString()).ToList();

				IList<FileInfo> exportedImagesWithSameControlNumber =
					exportedImages.Where(f => f.Name.Contains(document.ItemArray[0].ToString())).ToList();
				
				for (var i = 0; i < imagesWithSameControlNumber.Count; i++)
				{
					FileInfo file1 = exportedImagesWithSameControlNumber[i];
					var file2 = new FileInfo((string) imagesWithSameControlNumber[i].ItemArray[2]);

					Assert.IsTrue(FileComparer.Compare(file1, file2));
				}
			}
		}
	}
}