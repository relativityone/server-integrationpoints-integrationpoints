namespace kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile
{
    public class LoadFileSettingsModel
    {
        public ImportType ImportType { get; set; }

        public string WorkspaceDestinationFolder { get; set; }

        public string ImportSource { get; set; }

        public int StartLine { get; set; }
    }
}