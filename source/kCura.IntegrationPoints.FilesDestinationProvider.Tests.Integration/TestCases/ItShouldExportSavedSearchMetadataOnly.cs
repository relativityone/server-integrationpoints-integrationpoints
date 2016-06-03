using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
    internal class ItShouldExportSavedSearchMetadataOnly : IExportTestCase
    {
        private const string _METADATA_FORMAT = "dat";

        private string _expectedMetadataFilename;

        public ExportSettings Prepare(ExportSettings settings)
        {
            settings.ExportFilesLocation += $"_{nameof(ItShouldExportSavedSearchMetadataOnly)}";

            _expectedMetadataFilename = $"{settings.ExportedObjName}_export.{_METADATA_FORMAT}";
            
            return settings;
        }

        public void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
        {
            // verify that metadata file was created
            var actuals = directory.EnumerateFiles($"*.{_METADATA_FORMAT}", SearchOption.TopDirectoryOnly);
            var actual = actuals.FirstOrDefault();

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual?.Name, Is.EqualTo(_expectedMetadataFilename));
            Assert.That(actual?.Length, Is.GreaterThan(0));

            // verify that no other files were exported
            var numberOfOtherFiles = directory.EnumerateFiles("*", SearchOption.AllDirectories)
                .Count(f => !f.Name.Equals(_expectedMetadataFilename));

            Assert.That(numberOfOtherFiles, Is.EqualTo(0));
        }
    }
}
