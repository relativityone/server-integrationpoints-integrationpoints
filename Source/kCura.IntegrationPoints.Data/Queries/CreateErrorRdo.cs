using System;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class CreateErrorRdo
	{
		private readonly IRSAPIClient _client;
		public const int MAX_ERROR_LEN = 2000;
		private const string TRUNCATED_TEMPLATE = "(Truncated) {0}";

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
			var errDto = new kCura.Relativity.Client.DTOs.Error();
			var truncatedLength = MAX_ERROR_LEN - TRUNCATED_TEMPLATE.Length;
			errDto.Message = errorMessage.Length < MAX_ERROR_LEN ? errorMessage : string.Format(TRUNCATED_TEMPLATE, errorMessage.Substring(0, truncatedLength));
			errDto.FullError = stackTrace;
			errDto.Server = System.Environment.MachineName;
			errDto.Source = source;
			errDto.SendNotification = false;
			errDto.Workspace = new kCura.Relativity.Client.DTOs.Workspace(workspaceId);
			_client.Repositories.Error.Create(errDto);
		}

	}
}
