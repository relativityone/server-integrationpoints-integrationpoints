using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportImagesAsPdfs : IExportTestCase
	{
		public ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportFilesLocation += $"_{nameof(ItShouldExportImagesAsPdfs)}";

			settings.ExportImages = true;
			settings.CopyFileFromRepository = true;
			settings.ImageType = ExportSettings.ImageFileType.Pdf;

			return settings;
		}

		public void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var actualFiles = directory.EnumerateDirectories("IMAGES", SearchOption.AllDirectories)
				.SelectMany(d => d.EnumerateFiles("*", SearchOption.AllDirectories))
				.ToList();

			var expectedFiles = images
				.AsEnumerable()
				.GroupBy(r => r.Field<string>("DocumentIdentifier"))
				.Select(r => new FileInfo(Path.ChangeExtension(r.First().Field<string>("FileLocation"), ".pdf")))
				.ToList();

			Assert.That(actualFiles.Count, Is.Positive);
			Assert.That(actualFiles.Count, Is.EqualTo(expectedFiles.Count));
			Assert.That(actualFiles.Any(af => expectedFiles.Exists(ef => ef.Name.Equals(af.Name))));
		}
	}
}
