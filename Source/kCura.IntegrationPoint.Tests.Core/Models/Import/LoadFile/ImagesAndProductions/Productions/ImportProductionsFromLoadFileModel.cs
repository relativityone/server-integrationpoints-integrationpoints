namespace kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.ImagesAndProductions.Productions
{
    public class ImportProductionsFromLoadFileModel
    {
        public ImportSettingsModel ImportSettings { get; set; }

        public LoadFileSettingsModel LoadFileSettings { get; set; }

        public IntegrationPointGeneralModel General { get; set; }

        public ImportProductionsFromLoadFileModel(string name, string transferredObject)
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