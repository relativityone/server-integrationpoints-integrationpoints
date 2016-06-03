using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportImagesMetadataOnly : IExportTestCase
	{
		private const string _IMAGE_METADATA_FORMAT = "opt";

		private string _expectedMetadataFilename;

		public ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportFilesLocation += $"_{nameof(ItShouldExportImagesMetadataOnly)}";

			settings.ExportImages = true;
			settings.CopyFileFromRepository = false;

			_expectedMetadataFilename = $"{settings.ExportedObjName}_export.{_IMAGE_METADATA_FORMAT}";

			return settings;
		}

		public void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			// verify that metadata file was created
			var actual = directory.EnumerateFiles($"*.{_IMAGE_METADATA_FORMAT}", SearchOption.TopDirectoryOnly)
				.FirstOrDefault();

			Assert.That(actual, Is.Not.Null);
			Assert.That(actual?.Name, Is.EqualTo(_expectedMetadataFilename));
			Assert.That(actual?.Length, Is.Positive);

			// verify that no images were exported
			var unwantedFileExtensions = new[] { ".tif", ".tiff", ".jpg", ".jpeg", ".pdf" };

			var numberOfImages = directory.EnumerateFiles("*", SearchOption.AllDirectories)
				.Count(f => unwantedFileExtensions.Contains(f.Extension));

			Assert.That(numberOfImages, Is.EqualTo(0));
		}
	}
}
