namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class EntityExportToLoadFileModel : IntegrationPointGeneralModel
    {
        public EntityExportToLoadFileDetails ExportDetails { get; set; }

        public ExportToLoadFileOutputSettingsModel OutputSettings { get; set; }
        public EntityExportToLoadFileModel(string name) : base(name)
        {
            DestinationProvider = INTEGRATION_POINT_PROVIDER_LOADFILE;
        }
    }
}
