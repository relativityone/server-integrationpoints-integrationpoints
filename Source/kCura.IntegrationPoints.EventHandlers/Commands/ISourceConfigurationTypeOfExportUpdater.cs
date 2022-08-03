namespace kCura.IntegrationPoints.EventHandlers.Commands
{
    public interface ISourceConfigurationTypeOfExportUpdater
    {
        string GetCorrectedSourceConfiguration(int? sourceProvider, int? destinationProvider, string sourceConfiguration);
    }
}