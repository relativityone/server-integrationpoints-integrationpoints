using System;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class CreateErrorRdo
	{
		public const int MAX_ERROR_LEN = 2000;
		private const string _TRUNCATED_TEMPLATE = "(Truncated) {0}";
		private readonly IRSAPIClient _client;

		public CreateErrorRdo(IRSAPIClient client)
		{
			_client = client;
		}

		public virtual void Execute(int workspaceID, string source, Exception e)
		{
			Execute(workspaceID, source, e.Message, e.ToString());
		}

		public virtual void Execute(int workspaceId, string source, string errorMessage, string stackTrace)
		{
			var errDto = new Error();
			var truncatedLength = MAX_ERROR_LEN - _TRUNCATED_TEMPLATE.Length;
			errDto.Message = errorMessage.Length < MAX_ERROR_LEN ? errorMessage : string.Format(_TRUNCATED_TEMPLATE, errorMessage.Substring(0, truncatedLength));
			errDto.FullError = stackTrace;
			errDto.Server = Environment.MachineName;
			errDto.Source = source;
			errDto.SendNotification = false;
			errDto.Workspace = new Workspace(workspaceId);
			_client.Repositories.Error.Create(errDto);
		}
	}
}