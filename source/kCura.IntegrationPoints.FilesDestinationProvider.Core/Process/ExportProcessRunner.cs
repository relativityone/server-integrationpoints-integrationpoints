using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.WinEDDS;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
    public class ExportProcessRunner
    {
        public void StartWith(ExportSettings settings, IJobHistoryErrorService jobHistoryErrorService, IAPILog apiLog)
        {
            var searchExporter = new ExportProcessBuilder().Create(settings);

            AttachHandlers(searchExporter, jobHistoryErrorService, apiLog);
            searchExporter.ExportSearch();
        }

        private void AttachHandlers(Exporter exporter, IJobHistoryErrorService jobHistoryErrorService, IAPILog apiLog)
        {
            var exportUserNotification = new ExportUserNotification();
            var jobErrorLoggingMediator = new JobErrorLoggingMediator(jobHistoryErrorService);
            var exportLoggingMediator = new ExportLoggingMediator(apiLog);

            exporter.InteractionManager = exportUserNotification;

            jobErrorLoggingMediator.RegisterEventHandlers(exportUserNotification, exporter);
            exportLoggingMediator.RegisterEventHandlers(exportUserNotification, exporter);
        }
    }
}