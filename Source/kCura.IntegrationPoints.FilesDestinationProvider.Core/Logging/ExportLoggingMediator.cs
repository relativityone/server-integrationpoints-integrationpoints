using System;
using System.ComponentModel;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using Relativity.API;
using Relativity.DataExchange.Process;
using Relativity.DataExchange.Transfer;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
    public class ExportLoggingMediator : ILoggingMediator
    {
        private readonly IAPILog _apiLog;

        public ExportLoggingMediator(IAPILog apiLog)
        {
            _apiLog = apiLog;
        }

        public void RegisterEventHandlers(IUserMessageNotification userMessageNotification,
            ICoreExporterStatusNotification exporterStatusNotification)
        {
            userMessageNotification.UserFatalMessageEvent += OnUserFatalMessage;
            userMessageNotification.UserWarningMessageEvent += OnUserWarningMessage;
            exporterStatusNotification.FatalErrorEvent += OnFatalError;
            exporterStatusNotification.FileTransferMultiClientModeChangeEvent += OnFileTransferModeChange;
            exporterStatusNotification.StatusMessage += OnStatusMessage;
        }

        private void OnUserFatalMessage(object sender, UserMessageEventArgs userMessageEventArgs)
        {
            _apiLog.LogFatal(userMessageEventArgs.Message);
        }

        private void OnUserWarningMessage(object sender, UserMessageEventArgs userMessageEventArgs)
        {
            _apiLog.LogWarning(userMessageEventArgs.Message);
        }

        private void OnFileTransferModeChange(object sender, TapiMultiClientEventArgs args)
        {
            string mode = string.Join(",", args.TransferClients);
            _apiLog.LogInformation("File transfer mode has been changed: {mode}", mode);
        }

        private void OnFatalError(string message, Exception exception)
        {
            _apiLog.LogFatal(exception, message);
        }

        private void OnStatusMessage(ExportEventArgs exportEventArgs)
        {
            switch (exportEventArgs.EventType)
            {
                case EventType2.Status:
                    LogStatus(exportEventArgs);
                    return;
                case EventType2.Progress:
                    LogProgress(exportEventArgs);
                    return;
                case EventType2.Warning:
                    LogWarning(exportEventArgs);
                    return;
                case EventType2.Error:
                    LogError(exportEventArgs);
                    return;
                case EventType2.ResetStartTime:
                case EventType2.Count:
                case EventType2.End:
                    LogDebug(exportEventArgs);
                    return;
                case EventType2.Statistics:
                    LogStatitstics(exportEventArgs);
                    return;
                default:
                    throw new InvalidEnumArgumentException($"Unknown EventType ({exportEventArgs.EventType})");
            }
        }

        private void LogStatitstics(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogInformation("Statistics update: {message}.", exportEventArgs.Message);
        }

        private void LogStatus(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogInformation("Status update: {message}.", exportEventArgs.Message);
        }

        private void LogProgress(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogInformation("Progress update: {message}.", exportEventArgs.Message);
        }

        private void LogWarning(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogWarning("Warning: {message}.", exportEventArgs.Message);
        }

        private void LogError(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogError("Error occured: {message}.", exportEventArgs.Message);
        }

        private void LogDebug(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogInformation("Unexpected EventType.{event}. EventArgs: {@eventArgs}", exportEventArgs.EventType, exportEventArgs);
        }
    }
}
