using System;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using Relativity.DataExchange.Process;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
    public class JobErrorLoggingMediator : ILoggingMediator
    {
        private readonly IJobHistoryErrorService _historyErrorService;

        public JobErrorLoggingMediator(IJobHistoryErrorService historyErrorService)
        {
            _historyErrorService = historyErrorService;
        }

        public void RegisterEventHandlers(IUserMessageNotification userMessageNotification,
            ICoreExporterStatusNotification exporterStatusNotification)
        {
            userMessageNotification.UserFatalMessageEvent += OnUserFatalMessage;
            userMessageNotification.UserWarningMessageEvent += OnUserWarningMessage;
            exporterStatusNotification.FatalErrorEvent += OnFatalError;
            exporterStatusNotification.StatusMessage += OnStatusMessage;
        }

        private void OnStatusMessage(ExportEventArgs exportArgs)
        {
            if (exportArgs.EventType == EventType2.Error)
            {
                _historyErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, string.Empty, exportArgs.Message,
                    string.Empty);
            }
        }

        private void OnFatalError(string message, Exception ex)
        {
            _historyErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
        }

        private void OnUserFatalMessage(object sender, UserMessageEventArgs e)
        {
            _historyErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, e.Message, string.Empty);
        }

        private void OnUserWarningMessage(object sender, UserMessageEventArgs e)
        {
            _historyErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, string.Empty, e.Message, string.Empty);
        }
    }
}