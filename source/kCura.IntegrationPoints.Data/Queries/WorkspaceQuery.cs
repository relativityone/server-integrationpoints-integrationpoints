using System.Linq;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class WorkspaceQuery
	{
		private readonly IRSAPIClient _client;

		public WorkspaceQuery(IRSAPIClient client)
		{
			_client = client;
		}

		public Relativity.Client.DTOs.Workspace GetWorkspace(int workspaceID)
		{
			var workspace = _client.APIOptions.WorkspaceID;
			try
			{
				_client.APIOptions.WorkspaceID = -1;
				var result = _client.Repositories.Workspace.Read(workspaceID);
				RdoHelper.CheckResult(result);
				return result.Results.First().Artifact;
			}
			finally
			{
				_client.APIOptions.WorkspaceID = workspace;
			}
		}
	}
}
