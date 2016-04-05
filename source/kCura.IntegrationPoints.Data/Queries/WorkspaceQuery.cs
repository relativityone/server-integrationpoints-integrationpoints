using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class WorkspaceQuery
	{
		private readonly IRSAPIClient _client;

		public WorkspaceQuery(IRSAPIClient client)
		{
			_client = client;
		}

		public string GetWorkspaceName(int workspaceId)
		{
			var workspace = _client.APIOptions.WorkspaceID;
			try
			{
				_client.APIOptions.WorkspaceID = -1;

				WholeNumberCondition workspaceCondition = new WholeNumberCondition(ArtifactQueryFieldNames.ArtifactID, NumericConditionEnum.EqualTo, workspaceId);
				Query<Workspace> query = new Query<Workspace>
				{
					Condition = workspaceCondition,
					Fields = new List<FieldValue>() { new FieldValue() { Name = "Name" } }
				};

				QueryResultSet<Workspace> resultSet = _client.Repositories.Workspace.Query(query);
				RdoHelper.CheckResult(resultSet);
				return resultSet.Results.First().Artifact.Name;
			}
			finally
			{
				_client.APIOptions.WorkspaceID = workspace;
			}
		}
	}
}