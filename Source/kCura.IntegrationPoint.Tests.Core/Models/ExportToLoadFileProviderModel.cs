namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class ExportToLoadFileProviderModel : IntegrationPointGeneralModel
    {
        public ExportToLoadFileSourceInformationModel SourceInformationModel { get; set; }

        public ExportToLoadFileDetailsModel ExportDetails { get; set; }

        public ExportToLoadFileVolumeAndSubdirectoryModel ToLoadFileVolumeAndSubdirectoryModel { get; set; }

        public ExportToLoadFileOutputSettingsModel OutputSettings { get; set; }

        public ExportToLoadFileProviderModel(string name, string savedSearch) : base(name)
        {
            DestinationProvider = INTEGRATION_POINT_PROVIDER_LOADFILE;

            SourceInformationModel = new ExportToLoadFileSourceInformationModel(savedSearch);
            ExportDetails = new ExportToLoadFileDetailsModel();
            ToLoadFileVolumeAndSubdirectoryModel = new ExportToLoadFileVolumeAndSubdirectoryModel();
            OutputSettings = new ExportToLoadFileOutputSettingsModel();
        }

        public enum FilePathTypeEnum
        {
            Relative,
            Absolute,
            UserPrefix
        }

        public enum DestinationFolderTypeEnum
        {
            Root,
            SubfolderOfRoot
        }
    }
}
