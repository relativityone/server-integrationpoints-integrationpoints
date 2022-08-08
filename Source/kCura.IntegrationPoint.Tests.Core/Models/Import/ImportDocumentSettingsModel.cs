using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import
{
    public class ImportDocumentSettingsModel
    {
        public CopyNativeFiles CopyNativeFiles { get; set; }

        public string NativeFilePath { get; set; }

        public bool UseFolderPathInformation { get; set; }

        public string FolderPathInformation { get; set; }

        public bool CellContainsFileLocation { get; set; }

        public string FileLocationCell { get; set; }

        public string EncodingForUndetectableFiles { get; set; }
    }
}
