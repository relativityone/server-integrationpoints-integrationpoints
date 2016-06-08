using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
    internal class ItShouldExportDataFileAsCsv : BaseMetadataExportTestCase
    {
        public override ExportSettings Prepare(ExportSettings settings)
        {
            settings.OutputDataFileFormat = ExportSettings.DataFileFormat.CSV;

            return base.Prepare(settings);
        }

        public override void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
        {
            var fileInfo = GetFileInfo(directory);
            Assert.That(DataFileFormatHelper.FileStartWith("\"Control Number\"", fileInfo));
        }

        public override string MetadataFormat => "csv";
    }
}