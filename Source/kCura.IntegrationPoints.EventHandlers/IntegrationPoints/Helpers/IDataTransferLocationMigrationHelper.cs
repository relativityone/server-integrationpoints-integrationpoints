using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
    public interface IDataTransferLocationMigrationHelper
    {
        string GetUpdatedSourceConfiguration(string sourceConfiguration, IList<string> processingSourceLocations, string newDataTransferLocationRoot);
    }
}