namespace kCura.IntegrationPoints.Data.Queries
{
	using System;
	using kCura.Relativity.Client;

	public class GetSavedSearchesQuery
	{
		private readonly IRSAPIClient _client;

		public GetSavedSearchesQuery(IRSAPIClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Get all saved searches within specified workspace via RSAPI client.
		/// </summary>
		/// <returns>query result contains saved search artifact(s).</returns>
		public QueryResult ExecuteQuery()
		{
			var query = new Query();
			query.ArtifactTypeID = (Int32)ArtifactType.Search;
			return _client.Query(_client.APIOptions, query);
		}
	}
}