using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetSavedSearchesQuery
	{
		private IRSAPIClient _client;

		public GetSavedSearchesQuery(IRSAPIClient client)
		{
			_client = client;
		}

		public QueryResult ExecuteQuery()
		{
			var query = new Query();
			query.ArtifactTypeID = (Int32)ArtifactType.Search;
			return _client.Query(_client.APIOptions, query);
		}

	}
}
