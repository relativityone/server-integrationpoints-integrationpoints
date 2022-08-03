using System;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Agent.Logging
{
    public class ErrorService
    {
        private readonly CreateErrorRdoQuery _createErrorRdoQuery;

        public ErrorService(IHelper helper, ISystemEventLoggingService systemEventLoggingService)
        {
            _createErrorRdoQuery = new CreateErrorRdoQuery(helper.GetServicesManager(), systemEventLoggingService, helper.GetLoggerFactory().GetLogger().ForContext<ErrorService>());
        }

        public void LogError(Job job, Exception ex, string source)
        {
            LogError(job.WorkspaceID, source, ex.Message, ex.FlattenErrorMessagesWithStackTrace());
        }

        public void LogError(Job job, IntegrationPointsException ex)
        {
            if (ex.ShouldAddToErrorsTab)
            {
                string source = ex.ExceptionSource;
                LogError(job.WorkspaceID, source, ex.Message, ex.FlattenErrorMessagesWithStackTrace());
            }
        }

        private void LogError(int workspaceId, string source, string errorMessage, string stackTrace)
        {
            var error = new global::Relativity.Services.Error.Error
            {
                Message = errorMessage,
                FullError = stackTrace,
                Server = Environment.MachineName,
                Source = source,
                SendNotification = false,
                Workspace = new WorkspaceRef(workspaceId)
            };

            _createErrorRdoQuery.LogError(error);
        }
    }
}