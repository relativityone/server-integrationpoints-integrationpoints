using System.Collections.Generic;

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
		/// Get all saved searches within specified workspace via RSAPI client. Excludes the Temporary Retry Errors Searches that are made for item-level retries.
		/// </summary>
		/// <returns>query result contains saved search artifact(s).</returns>
		public QueryResult ExecuteQuery()
		{
			var query = new Query()
			{
				ArtifactTypeID = (Int32)ArtifactType.Search,
				Condition = new NotCondition(new TextCondition("Name", TextConditionEnum.Like, $"{Data.Constants.TEMPORARY_JOB_HISTORY_ERROR_SAVED_SEARCH_NAME}%")),
				Sorts = new List<Sort>()
				{
					new Sort()
					{
						Field = "Name",
						Direction = SortEnum.Ascending
					}
				}
			};

			return _client.Query(_client.APIOptions, query);
		}
	}
}