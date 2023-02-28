using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import
{
    public class SettingsModel
    {
        public OverwriteType Overwrite { get; set; } // TODO remove Type

        public MultiSelectFieldOverlayBehavior MultiSelectFieldOverlayBehavior { get; set; }

        public CopyNativeFiles CopyNativeFiles { get; set; }

        public string NativeFilePath { get; set; }

        public bool UseFolderPathInformation { get; set; }

        public string FolderPathInformation { get; set; }

        public bool MoveExistingDocuments { get; set; }

        public bool CellContainsFileLocation { get; set; }

        public string CellContainingFileLocation { get; set; }

        public string EncodingForUndetectableFiles { get; set; }
    }
}
