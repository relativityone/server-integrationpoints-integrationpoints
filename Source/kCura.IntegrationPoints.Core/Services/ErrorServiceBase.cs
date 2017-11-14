using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services
{
	public abstract class ErrorServiceBase : IErrorService
	{
		protected CreateErrorRdoQuery CreateErrorRdo { get; }

		public virtual string AppName { get; }

		public abstract string DefaultSourceName { get; }

		protected ErrorServiceBase(CreateErrorRdoQuery createError)
		{
			CreateErrorRdo = createError;
			string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
			AppName = $"Integration Points v({appVersion})";
		}

		public virtual void Log(ErrorModel error)
		{
			CreateErrorRdo.Execute(CreateErrorFromModel(error));
		}

		protected virtual Error CreateErrorFromModel(ErrorModel error)
		{
			return new Error
			{
				Message = error.Message,
				FullError = error.FullError,
				Source = FormatSource(error.Source),
				Server = Environment.MachineName,
				URL = error.Location,
				SendNotification = false,
				Workspace = new Workspace(error.WorkspaceId)
			};
		}

		private string FormatSource(string source)
		{
			return $"{AppName} : {source ?? DefaultSourceName}";
		}
	}
}