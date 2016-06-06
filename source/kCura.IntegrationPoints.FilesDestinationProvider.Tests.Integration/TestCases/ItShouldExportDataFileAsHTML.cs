using System.Data;
using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases
{
    internal class ItShouldExportDataFileAsHtml : IExportTestCase
    {
        public ExportSettings Prepare(ExportSettings settings)
        {
            settings.ExportFilesLocation += $"_{GetType().Name}";

            settings.OutputDataFileFormat = ExportSettings.DataFileFormat.HTML;

            return settings;
        }

        public void Verify(DirectoryInfo directory, DataTable documents, DataTable images)
        {
            var fileInfo = DataFileFormatHelper.GetFileInFormat("*.html", directory);
            Assert.IsNotNull(fileInfo);
            Assert.That(DataFileFormatHelper.FileStartWith("<html>", fileInfo));
        }
    }
}