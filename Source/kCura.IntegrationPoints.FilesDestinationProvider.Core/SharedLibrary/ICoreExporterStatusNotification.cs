using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface ICoreExporterStatusNotification : IExporterStatusNotification
    {
        event BatchCompleted OnBatchCompleted;
    }
}
