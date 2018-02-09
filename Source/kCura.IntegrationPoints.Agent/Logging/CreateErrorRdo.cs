using System;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Logging
{
	public class CreateErrorRdo
	{
		private readonly CreateErrorRdoQuery _createErrorRdoQuery;

		public CreateErrorRdo(IRsapiClientWithWorkspaceFactory rsapiClientFactory, IHelper helper, ISystemEventLoggingService systemEventLoggingService)
		{
			_createErrorRdoQuery = new CreateErrorRdoQuery(rsapiClientFactory, helper.GetLoggerFactory().GetLogger().ForContext<CreateErrorRdo>(), systemEventLoggingService);
		}

		public void Execute(Job job, Exception ex, string source)
		{
			Execute(job.WorkspaceID, source, ex.Message, ex.FlattenErrorMessages());
		}

		public void Execute(Job job, IntegrationPointsException ex)
		{
			if (ex.ShouldAddToErrorsTab)
			{
				string source = ex.ExceptionSource;
				Execute(job.WorkspaceID, source, ex.Message, ex.FlattenErrorMessages());
			}
		}

		private void Execute(int workspaceId, string source, string errorMessage, string stackTrace)
		{
			var error = new Error
			{
				Message = errorMessage,
				FullError = stackTrace,
				Server = Environment.MachineName,
				Source = source,
				SendNotification = false,
				Workspace = GetWorkspace(workspaceId)
			};

			_createErrorRdoQuery.Execute(error);
		}

		private static Workspace GetWorkspace(int workspaceId)
		{
			return new Workspace(workspaceId);
		}
	}
}