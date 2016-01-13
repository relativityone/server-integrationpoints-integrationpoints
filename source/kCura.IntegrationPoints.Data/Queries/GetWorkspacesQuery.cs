using System;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetWorkspacesQuery
	{
		private IRSAPIClient _client;

		public GetWorkspacesQuery(IRSAPIClient client)
		{
			_client = client;
		}

		public QueryResult ExecuteQuery()
		{
			var query = new Query();
			query.ArtifactTypeID = (Int32)ArtifactType.Case;
			return _client.Query(_client.APIOptions, query);
		}
	}
}