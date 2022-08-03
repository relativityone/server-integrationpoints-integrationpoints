using kCura.IntegrationPoint.Tests.Core.Models.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.ImagesAndProductions
{
    public abstract class ImportLoadFileImageProductionSettingsModel
    {
        public Numbering Numbering { get; set; }

        public OverwriteType ImportMode { get; set; }

        public bool CopyFilesToDocumentRepository { get; set; }

        public string FileRepository { get; set; }
    }
}