﻿using System;
using kCura.Windows.Process;
using kCura.WinEDDS;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
    public class ExportLoggingMediator
    {
        private readonly IAPILog _apiLog;

        public ExportLoggingMediator(IUserMessageNotification userMessageNotification,
            IExporterStatusNotification exporterStatusNotification, IAPILog apiLog)
        {
            _apiLog = apiLog;
            userMessageNotification.UserMessageEvent += OnUserMessage;
            exporterStatusNotification.FatalErrorEvent += OnFatalError;
            exporterStatusNotification.FileTransferModeChangeEvent += OnFileTransferModeChange;
            exporterStatusNotification.StatusMessage += OnStatusMessage;
        }

        private void OnUserMessage(object sender, UserMessageEventArgs userMessageEventArgs)
        {
            _apiLog.LogFatal(userMessageEventArgs.Message);
        }

        private void OnFileTransferModeChange(string newMode)
        {
            _apiLog.LogInformation("File transfer mode has been changed: {mode}", newMode);
        }

        private void OnFatalError(string message, Exception exception)
        {
            _apiLog.LogFatal(exception, message);
        }

        private void OnStatusMessage(ExportEventArgs exportEventArgs)
        {
            switch (exportEventArgs.EventType)
            {
                case EventType.Status:
                    LogStatus(exportEventArgs);
                    return;
                case EventType.Progress:
                    LogProgress(exportEventArgs);
                    return;
                case EventType.Warning:
                    LogWarning(exportEventArgs);
                    return;
                case EventType.Error:
                    LogError(exportEventArgs);
                    return;
                case EventType.ResetStartTime:
                case EventType.Count:
                case EventType.End:
                    LogDebug(exportEventArgs);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LogStatus(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogInformation("Status update: {message}. Additional info: {@additionalInfo}.",
                exportEventArgs.Message, exportEventArgs.AdditionalInfo);
        }

        private void LogProgress(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogInformation("Progress update: {message}. Additional info: {@additionalInfo}.",
                exportEventArgs.Message, exportEventArgs.AdditionalInfo);
        }

        private void LogWarning(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogWarning("Warning: {message}. Additional info: {@additionalInfo}.",
                exportEventArgs.Message, exportEventArgs.AdditionalInfo);
        }

        private void LogError(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogError("Error occured: {message}. Additional info: {@additionalInfo}.",
                exportEventArgs.Message, exportEventArgs.AdditionalInfo);
        }

        private void LogDebug(ExportEventArgs exportEventArgs)
        {
            _apiLog.LogDebug("Unexpected EventType.{event}. EventArgs: {@eventArgs}", exportEventArgs.EventType,
                exportEventArgs);
        }
    }
}