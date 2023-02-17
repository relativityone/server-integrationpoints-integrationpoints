namespace kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.ImagesAndProductions.Images
{
    public class ImportImagesFromLoadFileModel
    {
        public ImportSettingsModel ImportSettings { get; set; }

        public LoadFileSettingsModel LoadFileSettings { get; set; }

        public IntegrationPointGeneralModel General { get; set; }

        public ImportImagesFromLoadFileModel(string name, string transferredObject)
        {
            General = new IntegrationPointGeneralModel(name)
            {
                Type = IntegrationPointType.Import,
                SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE,
                TransferredObject = transferredObject,
            };

            LoadFileSettings = new LoadFileSettingsModel();

            ImportSettings = new ImportSettingsModel();
        }

    }
}
