using System;
using System.Reflection;
using System.Diagnostics;
using Relativity.API;
using Relativity.Services.Error;
using Relativity.Services.Workspace;
using kCura.Utility.Extensions;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services
{
    public abstract class ErrorServiceBase : IErrorService
    {
        private readonly IAPILog _log;
        protected CreateErrorRdoQuery CreateErrorRdo { get; }

        public virtual string AppName { get; }

        public abstract string TargetName { get; }

        protected ErrorServiceBase(CreateErrorRdoQuery createError, IAPILog log)
        {
            _log = log;
            CreateErrorRdo = createError;
            string appVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            AppName = $"Integration Points v({appVersion})";
        }

        public virtual void Log(ErrorModel error)
        {
            if (error.AddToErrorTab)
            {
                CreateErrorRdo.LogError(CreateErrorFromModel(error));
            }
            string sourceContent = FormatSourceContent(error.Source);
            string errorMessage = error.Message;
            _log.LogError(error.Exception, "{sourceContent} {errorMessage}", sourceContent, errorMessage);
        }

        protected virtual Error CreateErrorFromModel(ErrorModel error)
        {
            return new Error
            {
                Message = AppendAdditionalInfo(error),
                FullError = error.FullError,
                Source = FormatSourceContent(error.Source),
                Server = Environment.MachineName,
                URL = error.Location,
                SendNotification = false,
                Workspace = new WorkspaceRef(error.WorkspaceId)
            };
        }

        private string AppendAdditionalInfo(ErrorModel error)
        {
            if (error.CorrelationId != null)
            {
                return $"{error.Message} - (Log Correlation Id: {error.CorrelationId})";
            }
            return error.Message;
        }

        private string FormatSourceContent(string source)
        {
            string formatSource = source.IsNullOrEmpty() ? string.Empty : $" - {source}";
            // eg: "Integration Points v(x.x.x.x) Custom Page - RsApi"
            return $"{AppName} {TargetName}{formatSource}";
        }
    }
}