namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
    internal class DefaultExportSettingsFactory
    {
        public static ExportSettings Create()
        {
            return new ExportSettings
            {
                ExportedObjName = string.Empty
            };
        }
    }
}