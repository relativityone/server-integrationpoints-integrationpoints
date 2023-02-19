namespace kCura.IntegrationPoint.Tests.Core.Models.Import.FTP
{
    public class ImportFromFtpModel
    {
        public ImportFromFtpModel(string name, string transferredObject)
        {
            General = new IntegrationPointGeneralModel(name)
            {
                Type = IntegrationPointType.Import,
                SourceProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_SOURCE_PROVIDER_FTP,
                TransferredObject = transferredObject,
            };

            ConnectionAndFileInfo = new ConnectionAndFileInfoModel();

            FieldsMapping = new FieldsMappingModel();

            Settings = new SettingsModel();
        }

        public IntegrationPointGeneralModel General { get; set; }

        public ConnectionAndFileInfoModel ConnectionAndFileInfo { get; set; }

        public FieldsMappingModel FieldsMapping { get; set; }

        public SettingsModel Settings { get; set; }

    }
}
