using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SavedSearchRepository : ISavedSearchRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;
		private readonly int _savedSearchId;
		private readonly int _pageSize;
		private string _queryToken = null;
		private int _documentsRetrieved = 0;
		private int _totalDocumentsRetrieved = 0;
		public bool StartedRetrieving = false;

		public SavedSearchRepository(
			IHelper helper,
			int workspaceArtifactId, 
			int savedSearchId, 
			int pageSize)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
			_savedSearchId = savedSearchId;
			_pageSize = pageSize;
		}

		public ArtifactDTO[] RetrieveNextDocuments()
		{
			StartedRetrieving = true;

			var query = new Query<kCura.Relativity.Client.DTOs.Document>
			{
				Condition = new SavedSearchCondition(_savedSearchId),
				Fields = new List<FieldValue>()
				{
					new FieldValue(ArtifactFieldNames.TextIdentifier)
				}
			};

			QueryResultSet<kCura.Relativity.Client.DTOs.Document> resultSet;
			if (_queryToken == null)
			{
				using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

					resultSet = rsapiClient.Repositories.Document.Query(query, _pageSize);
				}
			}
			else
			{
				using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
				{
					rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

					resultSet = rsapiClient.Repositories.Document.QuerySubset(_queryToken, _documentsRetrieved + 1, _pageSize);
				}
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

			throw new Exception($"Failed to retrieve for saved search ID {_savedSearchId}");
		}

		public bool AllDocumentsRetrieved()
		{
			return StartedRetrieving && (string.IsNullOrEmpty(_queryToken) || _totalDocumentsRetrieved - _documentsRetrieved == 0);
		}
	}
}