using System;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.Relativity.Client;

namespace kCura.ScheduleQueue.Core.Logging
{
	public class CreateErrorRdo
	{
		private readonly IRSAPIClient service;

		public CreateErrorRdo(IRSAPIClient service)
		{
			this.service = service;
		}

		public void Execute(Job job, Exception ex, string source)
		{
			Execute(job.WorkspaceID, source, ex.Message, ex.FlattenErrorMessages());
		}

		public void Execute(int workspaceId, string source, string errorMessage, string stackTrace)
		{
			var errDTO = new kCura.Relativity.Client.DTOs.Error();
			errDTO.Message = errorMessage.Length < 2000
				? errorMessage
				: String.Format("(Truncated) {0}", errorMessage.Substring(0, 1980));
			errDTO.FullError = stackTrace;
			errDTO.Server = System.Environment.MachineName;
			errDTO.Source = source;
			errDTO.SendNotification = false;
			if (workspaceId == 0)
			{
				workspaceId = -1;
			}
			errDTO.Workspace = new kCura.Relativity.Client.DTOs.Workspace(workspaceId);
			try
			{
				service.Repositories.Error.Create(errDTO);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: {0}", ex.Message);
				SystemEventLoggingService.WriteErrorEvent(source, "Application", ex);
			}
		}
	}
}
