using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client.DTOs;
using kCura.Utility.Extensions;
using Relativity.API;

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
			string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
			AppName = $"Integration Points v({appVersion})";
		}

		public virtual void Log(ErrorModel error)
		{
			if (error.AddToErrorTab)
			{
				CreateErrorRdo.Execute(CreateErrorFromModel(error));
			}
			string sourceContent = FormatSourceContent(error.Source);
			_log.LogError(error.Exception, "{sourceContent} {error.Message}", sourceContent, error.Message);
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
				Workspace = new Workspace(error.WorkspaceId)
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