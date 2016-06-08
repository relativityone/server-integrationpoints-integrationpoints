using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
    internal class ItShouldExportSavedSearchMetadataOnly : BaseMetadataExportTestCase
    {
        public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
        {
            // verify that metadata file was created
            var actual = GetFileInfo(directory);

            var expectedMetadataFilename = $"{ExportSettings.ExportedObjName}_export.{MetadataFormat}";

			Assert.That(actual, Is.Not.Null);
            Assert.That(actual?.Name, Is.EqualTo(expectedMetadataFilename));
            Assert.That(actual?.Length, Is.GreaterThan(0));

            // verify that no other files were exported
            var numberOfOtherFiles = directory.EnumerateFiles("*", SearchOption.AllDirectories)
                .Count(f => !f.Name.Equals(expectedMetadataFilename));

            Assert.That(numberOfOtherFiles, Is.EqualTo(0));
        }

		public override string MetadataFormat => "dat";
	}
}
