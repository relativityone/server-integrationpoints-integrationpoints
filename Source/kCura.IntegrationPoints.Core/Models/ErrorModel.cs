using System;
using kCura.IntegrationPoints.Domain.Exceptions;

namespace kCura.IntegrationPoints.Core.Models
{
	public class ErrorModel
	{
		public int WorkspaceId { get; set; }

		public string Message { get; set; }

		public string FullError { get; set; }

		public string Source { get; set; }

		public string Location { get; set; }

		public ErrorModel() { }

		public ErrorModel(Exception exception, string message = null)
		{
			Message = message ?? exception.Message;
			FullError = exception.ToString();
		}
	}
}
