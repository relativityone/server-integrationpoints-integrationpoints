namespace kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.Documents
{
    public class ImportDocumentsModel
    {
        public FileEncodingModel FileEncoding { get; set; }

        public FieldsMappingModel FieldsMapping { get; set; }

        public SettingsModel Settings { get; set; }

        public LoadFileSettingsModel LoadFileSettings { get; set; }

        public IntegrationPointGeneralModel General { get; set; }
        
        public ImportDocumentsModel(string name, string transferredObject)
        {
            General = new IntegrationPointGeneralModel(name)
            {
                Type = IntegrationPointType.Import,
                SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE,
                TransferredObject = transferredObject,
            };

            LoadFileSettings = new LoadFileSettingsModel();

            FileEncoding = new FileEncodingModel();

            FieldsMapping = new FieldsMappingModel();

            Settings = new SettingsModel();
        }
    }
}