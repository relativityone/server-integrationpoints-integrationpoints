using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
	internal class ItShouldExportImagesAsSinglePages : ExportTestCaseBase
	{
		public override ExportSettings Prepare(ExportSettings settings)
		{
			settings.ExportImages = true;
			settings.ExportNatives = true;
			settings.ImageType = ExportSettings.ImageFileType.SinglePage;

			return base.Prepare(settings);
		}

		public override void Verify(DirectoryInfo directory, DocumentsTestData documentsTestData)
		{
			var actualFiles = directory.EnumerateDirectories("IMAGES", SearchOption.AllDirectories)
				.SelectMany(d => d.EnumerateFiles("*", SearchOption.AllDirectories))
				.ToList();

			var expectedFiles = documentsTestData.Images
				.AsEnumerable()
				.Select(r => new FileInfo(r.Field<string>("FileLocation")))
				.ToList();

			Assert.That(actualFiles.Count, Is.GreaterThan(0));
			Assert.That(actualFiles.Count, Is.EqualTo(expectedFiles.Count));
			Assert.That(actualFiles.Any(af => expectedFiles.Exists(ef => ef.Name.Equals(af.Name))));
		}
	}
}