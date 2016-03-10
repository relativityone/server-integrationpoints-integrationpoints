using System;
using System.Collections.Generic;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetWorkspacesQuery
	{
		private readonly IRSAPIClient _client;

		public GetWorkspacesQuery(IRSAPIClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Get all workspaces
		/// </summary>
		/// <returns>query result contains workspace artifact(s).</returns>
		public QueryResultSet<Workspace> ExecuteQuery()
		{
			var workspaceQuery = new Query<Workspace>
			{
				Fields = new List<FieldValue>() { new FieldValue() { Name = "Name"} },
				Sorts = new List<Sort>() {
					new Sort()
					{
						Field = "Name",
						Direction = SortEnum.Ascending
					}
				}
			};

			return _client.Repositories.Workspace.Query(workspaceQuery);
		}
	}
}