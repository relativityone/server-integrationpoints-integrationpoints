using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiSavedSearchRepository : ISavedSearchRepository
	{
		private readonly IRSAPIClient _rsapiClient;
		private readonly int _savedSearchId;
		private readonly int _pageSize;
		private string _queryToken = null;
		private int _documentsRetrieved = 0;
		private int _totalDocumentsRetrieved = 0;

		public RsapiSavedSearchRepository(IRSAPIClient rsapiClient, int savedSearchId, int pageSize)
		{
			_rsapiClient = rsapiClient;
			_savedSearchId = savedSearchId;
			_pageSize = pageSize;
		}

		public ArtifactDTO[] RetrieveNext()
		{
			var query = new Query<Document>
			{
				Condition = new SavedSearchCondition(_savedSearchId),
				Fields = new List<FieldValue>()
				{
					new FieldValue(ArtifactFieldNames.TextIdentifier)
				}
			};

			QueryResultSet<Document> resultSet;
			if (_queryToken == null)
			{
				resultSet = _rsapiClient.Repositories.Document.Query(query, _pageSize);
			}
			else
			{
				resultSet = _rsapiClient.Repositories.Document.QuerySubset(_queryToken, _documentsRetrieved + 1, _pageSize);
			}

			if (resultSet != null && resultSet.Success)
			{
				_queryToken = resultSet.QueryToken;
				_totalDocumentsRetrieved = resultSet.TotalCount;

				ArtifactDTO[] results = resultSet.Results.Select(
					x => new ArtifactDTO(
						x.Artifact.ArtifactID,
						x.Artifact.ArtifactTypeID.GetValueOrDefault(),
						x.Artifact.TextIdentifier,
						new ArtifactFieldDTO[0])).ToArray();

				_documentsRetrieved += results.Length;

				return results;
			}

			throw new Exception(String.Format("Failed to retrieve for saved search ID {0}", _savedSearchId));
		}

		public bool AllDocumentsRetrieved()
		{
			return String.IsNullOrEmpty(_queryToken) || _totalDocumentsRetrieved - _documentsRetrieved == 0;
		}
	}
}