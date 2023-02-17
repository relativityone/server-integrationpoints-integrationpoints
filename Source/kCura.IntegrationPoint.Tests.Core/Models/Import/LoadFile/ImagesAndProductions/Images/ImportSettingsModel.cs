namespace kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.ImagesAndProductions.Images
{
    public class ImportSettingsModel : ImportLoadFileImageProductionSettingsModel
    {
        public bool LoadExtractedText { get; set; }

        public string EncodingForUndetectableFiles { get; set; }
    }
}
