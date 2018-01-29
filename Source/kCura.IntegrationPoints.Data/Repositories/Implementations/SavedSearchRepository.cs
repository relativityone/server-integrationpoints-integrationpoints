using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SavedSearchRepository : ISavedSearchRepository
	{
		private readonly IRelativityObjectManager _objectManager;
		private readonly int _savedSearchId;
		private readonly int _pageSize;
		private int _documentsRetrieved = 0;
		private int _totalDocumentsRetrieved = 0;
		public bool StartedRetrieving = false;
		private int currentPage = 0;

		public SavedSearchRepository(
			IRelativityObjectManager objectManager,
			int savedSearchId,
			int pageSize)
		{
			_objectManager = objectManager;
			_savedSearchId = savedSearchId;
			_pageSize = pageSize;
		}

		public ArtifactDTO[] RetrieveNextDocuments()
		{
			StartedRetrieving = true;

			var request = new QueryRequest
			{
				Fields = new List<FieldRef>
				{
					new FieldRef { Name = ArtifactFieldNames.TextIdentifier }
				},
				Condition = $"'ArtifactId' IN SAVEDSEARCH {_savedSearchId}"
			};

			UtilityDTO.ResultSet<Document> resultSet = _objectManager.Query<Document>(request, currentPage * _pageSize, _pageSize);

			if (resultSet != null && resultSet.Items.Any())
			{
				_totalDocumentsRetrieved = resultSet.Items.Count;

				ArtifactDTO[] results = resultSet.Items.Select(
					x => new ArtifactDTO(
						x.Rdo.ArtifactID,
						x.Rdo.ArtifactTypeID.GetValueOrDefault(),
						x.Rdo.TextIdentifier,
						new ArtifactFieldDTO[0])).ToArray();

				_documentsRetrieved += results.Length;

				return results;
			}

			throw new IntegrationPointsException($"Failed to retrieve for saved search ID {_savedSearchId}")
			{
				ExceptionSource = IntegrationPointsExceptionSource.GENERIC
			};
		}

		public bool AllDocumentsRetrieved()
		{
			return StartedRetrieving && _totalDocumentsRetrieved == _documentsRetrieved;
		}
	}
}