namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class ExportToLoadFileOutputSettingsModel
    {
        public ExportToLoadFileLoadFileOptionsModel LoadFileOptions { get; set; } = new ExportToLoadFileLoadFileOptionsModel();

        public ExportToLoadFileImageOptionsModel ImageOptions { get; set; } = new ExportToLoadFileImageOptionsModel();

        public ExportToLoadFileNativeOptionsModel NativeOptions { get; set; } = new ExportToLoadFileNativeOptionsModel();

        public ExportToLoadFileTextOptionsModel TextOptions { get; set; } = new ExportToLoadFileTextOptionsModel();

        public ExportToLoadFileVolumeAndSubdirectoryModel VolumeAndSubdirectoryOptions { get; set; } = new ExportToLoadFileVolumeAndSubdirectoryModel();
    }
}
