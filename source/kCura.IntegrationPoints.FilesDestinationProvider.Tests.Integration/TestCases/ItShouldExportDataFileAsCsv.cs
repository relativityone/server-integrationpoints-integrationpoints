using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
    internal class ItShouldExportDataFileAsCsv : IExportTestCase
    {
        public ExportSettings Prepare(ExportSettings settings)
        {
            settings.ExportFilesLocation += $"_{GetType().Name}";

            settings.OutputDataFileFormat = ExportSettings.DataFileFormat.CSV;

            return settings;
        }

        public void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
        {
            var fileInfo = DataFileFormatHelper.GetFileInFormat("*.csv", directory);
            Assert.IsNotNull(fileInfo);
            Assert.That(DataFileFormatHelper.FileStartWith("\"Control Number\"", fileInfo));
        }
    }
}