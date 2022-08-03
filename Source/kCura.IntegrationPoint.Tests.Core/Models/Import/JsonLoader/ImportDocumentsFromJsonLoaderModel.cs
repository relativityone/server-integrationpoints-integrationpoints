using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.JsonLoader
{
    public class ImportDocumentsFromJsonLoaderModel
    {
        public FieldsMappingModel FieldsMapping { get; set; }

        public SettingsModel Settings { get; set; }

        public JsonLoaderSettingsModel JsonLoaderSettings { get; set; }

        public IntegrationPointGeneralModel General { get; set; }

        public ImportDocumentsFromJsonLoaderModel(string name)
        {
            General = new IntegrationPointGeneralModel(name)
            {
                Type = IntegrationPointType.Import,
                SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_SOURCE_PROVIDER_JSON,
                TransferredObject = TransferredObjectConstants.JSON_OBJECT,
            };

            JsonLoaderSettings = new JsonLoaderSettingsModel();

            FieldsMapping = new FieldsMappingModel();

            Settings = new SettingsModel();
        }
    }
}