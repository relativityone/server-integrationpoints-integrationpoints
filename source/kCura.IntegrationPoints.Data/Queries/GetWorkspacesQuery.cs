using System;
using kCura.Relativity.Client;

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
		/// Get all workspaces within specified workspace via RSAPI client.
		/// </summary>
		/// <returns>query result contains workspace artifact(s).</returns>
		public QueryResult ExecuteQuery()
		{
			var query = new Query();
			query.ArtifactTypeID = (Int32)ArtifactType.Case;
			return _client.Query(_client.APIOptions, query);
		}
	}
}