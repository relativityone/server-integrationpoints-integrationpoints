using System;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Logging
{
	public class CreateErrorRdo
	{
		private readonly IRSAPIClient service;
		private readonly IAPILog _logger;

		public CreateErrorRdo(IRSAPIClient service, IHelper helper)
		{
			this.service = service;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<CreateErrorRdo>();
		}

		public void Execute(Job job, Exception ex, string source)
		{
			Execute(job.WorkspaceID, source, ex.Message, ex.FlattenErrorMessages());
		}

		public void Execute(int workspaceId, string source, string errorMessage, string stackTrace)
		{
			var errDTO = new Relativity.Client.DTOs.Error();
			errDTO.Message = errorMessage.Length < 2000
				? errorMessage
				: String.Format("(Truncated) {0}", errorMessage.Substring(0, 1980));
			errDTO.FullError = stackTrace;
			errDTO.Server = Environment.MachineName;
			errDTO.Source = source;
			errDTO.SendNotification = false;
			if (workspaceId == 0)
			{
				workspaceId = -1;
			}
			errDTO.Workspace = new Relativity.Client.DTOs.Workspace(workspaceId);
			try
			{
				service.Repositories.Error.Create(errDTO);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: {0}", ex.Message);
				SystemEventLoggingService.WriteErrorEvent(source, "Application", ex);

				string message =
					$"An Error occured during creation of ErrorRDO for workspace: {workspaceId}. {Environment.NewLine}Source: {source}. {Environment.NewLine}Error message: {errorMessage}. {Environment.NewLine}Stacktrace: {stackTrace}";

				_logger.LogError(ex, message);
			}
		}
	}
}