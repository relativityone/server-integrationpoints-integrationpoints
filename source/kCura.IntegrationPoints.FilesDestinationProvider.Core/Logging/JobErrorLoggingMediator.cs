using System;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.Windows.Process;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
    public class JobErrorLoggingMediator
    {
        private readonly IJobHistoryErrorService _historyErrorService;

        public JobErrorLoggingMediator(IUserMessageNotification userMessageNotification,
            IExporterStatusNotification exporterStatusNotification, IJobHistoryErrorService historyErrorService)
        {
            _historyErrorService = historyErrorService;
            userMessageNotification.UserMessageEvent += OnUserMessage;
            exporterStatusNotification.FatalErrorEvent += OnFatalError;
            exporterStatusNotification.StatusMessage += OnStatusMessage;
        }

        private void OnStatusMessage(ExportEventArgs exportArgs)
        {
            if (exportArgs.EventType == EventType.Error)
            {
                _historyErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, string.Empty, exportArgs.Message,
                    string.Empty);
            }
        }

        private void OnFatalError(string message, Exception ex)
        {
            _historyErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
        }

        private void OnUserMessage(object sender, UserMessageEventArgs e)
        {
            _historyErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, e.Message, string.Empty);
        }
    }
}