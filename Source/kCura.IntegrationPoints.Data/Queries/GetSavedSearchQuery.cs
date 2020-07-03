#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetSavedSearchQuery
	{
		private readonly IRSAPIClient _client;
		private readonly int _savedSearchArtifactId;

		public GetSavedSearchQuery(IRSAPIClient client, int savedSearchArtifactId)
		{
			_client = client;
			_savedSearchArtifactId = savedSearchArtifactId;
		}

		public QueryResult ExecuteQuery()
		{
			var query = new Query
			{
				ArtifactTypeID = (int) ArtifactType.Search,
				Condition = new WholeNumberCondition(ArtifactQueryFieldNames.ArtifactID, NumericConditionEnum.EqualTo, _savedSearchArtifactId)
			};
			return _client.Query(_client.APIOptions, query);
		}
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
