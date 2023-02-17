namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
    internal static class DefaultExportSettingsFactory
    {
        public static ExportSettings Create()
        {
            return new ExportSettings
            {
                SavedSearchName = string.Empty,
                ViewName = string.Empty,
                ProductionName = string.Empty
            };
        }
    }
}
