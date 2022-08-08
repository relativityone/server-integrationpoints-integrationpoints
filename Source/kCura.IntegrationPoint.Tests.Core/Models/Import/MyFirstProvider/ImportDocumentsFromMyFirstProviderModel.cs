using kCura.IntegrationPoint.Tests.Core.Models.Constants.Shared;

namespace kCura.IntegrationPoint.Tests.Core.Models.Import.MyFirstProvider
{
    public class ImportDocumentsFromMyFirstProviderModel
    {
        public FieldsMappingModel FieldsMapping { get; set; }

        public SettingsModel Settings { get; set; }

        public MyFirstProviderSettingsModel MyFirstProviderSettings { get; set; }

        public IntegrationPointGeneralModel General { get; set; }

        public ImportDocumentsFromMyFirstProviderModel(string name)
        {
            General = new IntegrationPointGeneralModel(name)
            {
                Type = IntegrationPointType.Import,
                SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_SOURCE_PROVIDER_MY_FIRST_PROVIDER,
                TransferredObject = TransferredObjectConstants.DOCUMENT,
            };

            MyFirstProviderSettings = new MyFirstProviderSettingsModel();

            FieldsMapping = new FieldsMappingModel();

            Settings = new SettingsModel();
        }
    }
}
