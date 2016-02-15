using System;

namespace kCura.IntegrationPoints.Core.Models
{
	public class ErrorModel
	{
		public int WorkspaceID { get; set; }
		public string Message { get; set; }
		public Exception Exception { get; set; }

		public ErrorModel(int workspaceID, string message, Exception exception)
		{
			this.WorkspaceID = workspaceID;
			this.Message = message;
			this.Exception = exception;
		}
	}
}
