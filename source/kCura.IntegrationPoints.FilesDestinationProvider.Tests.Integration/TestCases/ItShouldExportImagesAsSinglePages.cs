using System.Data;
using System.IO;
using System.Linq;
using kCura.EDDS.WebAPI.FileManagerBase;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportImagesAsSinglePages : BaseExportTestCase
    {
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = true;
			settings.CopyFileFromRepository = true;
			settings.ImageType = ExportSettings.ImageFileType.SinglePage;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
		{
			var actualFiles = directory.EnumerateDirectories("IMAGES", SearchOption.AllDirectories)
				.SelectMany(d => d.EnumerateFiles("*", SearchOption.AllDirectories))
				.ToList();

			var expectedFiles = images
				.AsEnumerable()
				.Select(r => new FileInfo(r.Field<string>("FileLocation")))
				.ToList();

			Assert.That(actualFiles.Count, Is.Positive);
			Assert.That(actualFiles.Count, Is.EqualTo(expectedFiles.Count));
			Assert.That(actualFiles.Any(af => expectedFiles.Exists(ef => ef.Name.Equals(af.Name))));
		}
	}
}
