using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.O365
{
    public class ImportDocumentsFromO365Model
    {
        public FieldsMappingModel FieldsMapping { get; set; }

        public SettingsModel Settings { get; set; }

        public O365SettingsModel O365Settings { get; set; }

        public IntegrationPointGeneralModel General { get; set; }

        public ImportDocumentsFromO365Model(string name)
        {
            General = new IntegrationPointGeneralModel(name)
            {
                Type = IntegrationPointType.Import,
                SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_SOURCE_PROVIDER_O365,
                TransferredObject = TransferredObjectConstants.DOCUMENT,
            };

            O365Settings = new O365SettingsModel();

            FieldsMapping = new FieldsMappingModel();

            Settings = new SettingsModel();
        }
    }
}
