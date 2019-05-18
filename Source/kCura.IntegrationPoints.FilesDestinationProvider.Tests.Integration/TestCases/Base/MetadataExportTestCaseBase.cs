using System.IO;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases.Base
{
    public abstract class MetadataExportTestCaseBase : ExportTestCaseBase
    {
        public abstract string MetadataFormat { get; }

        public string FileFormat => $"*.{MetadataFormat}";

        protected FileInfo GetFileInfo(DirectoryInfo directory)
        {
            return DataFileFormatHelper.GetFileInFormat(FileFormat, directory);
        }
    }
}
