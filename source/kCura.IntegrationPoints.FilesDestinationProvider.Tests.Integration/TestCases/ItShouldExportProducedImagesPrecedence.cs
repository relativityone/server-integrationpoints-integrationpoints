﻿using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	[Ignore("Ignored till problems with ProductionsAPI will be resolved")]
	public class ItShouldExportProducedImagesPrecedence : ExportTestCaseBase
	{
		private readonly string _defaultPlaceholderPath = Path.Combine(TestContext.CurrentContext.TestDirectory,
			@"TestData\DefaultPlaceholder.tif");

		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.CopyFileFromRepository = true;
			settings.ExportImages = true;
			settings.ProductionPrecedence = ExportSettings.ProductionPrecedenceType.Produced;


			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var defaultPlaceholderFile = new FileInfo(_defaultPlaceholderPath);

			var imagesDirectory = directory.GetDirectories("IMAGES", SearchOption.AllDirectories).First();
			var allExportedImages = imagesDirectory.GetFiles("*.*", SearchOption.AllDirectories);
			var totalExportedImagesCount = allExportedImages.Length;

			var expectedFilesWithImagesCount = images.Rows.Count;

			var documentsWithImages = documents.Select($"[Has Images] = '{true}'");
			var documentsWithoutImages = documents.Select($"[Has Images] = '{false}'");
			var numberofDocumentsWithoutImages = documentsWithoutImages.Length;

			Assert.AreEqual(totalExportedImagesCount, numberofDocumentsWithoutImages + expectedFilesWithImagesCount);

			AssertExportedImagesAreSameAsInputImages(documentsWithImages, images.Select(), allExportedImages);
			AssertExportedFilesWithoutImagesAreSameAsPlaceholder(documentsWithoutImages, allExportedImages,
				defaultPlaceholderFile);
		}

		private void AssertExportedFilesWithoutImagesAreSameAsPlaceholder(DataRow[] documentsWithoutImages,
			FileInfo[] exportedImages, FileInfo defaultPlaceholderFile)
		{
			foreach (var document in documentsWithoutImages)
			{
				var file =
					exportedImages.SingleOrDefault(f => Path.GetFileNameWithoutExtension(f.Name) == document.ItemArray[0].ToString());

				Assert.IsTrue(FileComparer.Compare(file, defaultPlaceholderFile));
			}
		}

		private void AssertExportedImagesAreSameAsInputImages(DataRow[] documentsWithoutImages, DataRow[] inputImages,
			FileInfo[] exportedImages)
		{
			foreach (var document in documentsWithoutImages)
			{
				var imagesWithSameControlNumber =
					inputImages.Where(x => x.ItemArray[0].ToString() == document.ItemArray[0].ToString()).ToList();

				var exportedImagesWithSameControlNumber =
					exportedImages.Where(f => f.Name.Contains(document.ItemArray[0].ToString())).ToList();
				
				for (var i = 0; i < imagesWithSameControlNumber.Count(); i++)
				{
					var file1 = exportedImagesWithSameControlNumber[i];
					var file2 = new FileInfo((string) imagesWithSameControlNumber[i].ItemArray[2]);

					Assert.IsTrue(FileComparer.Compare(file1, file2));
				}
			}
		}
	}
}