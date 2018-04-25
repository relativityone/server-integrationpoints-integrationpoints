using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SavedSearchQueryRepository : KeplerServiceBase, ISavedSearchQueryRepository
	{
		private const string _NAME_FIELD = "Name";
		private const string _OWNER_FIELD = "Owner";

		public SavedSearchQueryRepository(IRelativityObjectManager relativityObjectManager) : base(relativityObjectManager)
		{
		}

		public SavedSearchDTO RetrieveSavedSearch(int savedSearchId)
		{
			string condition = $"'Artifact ID' == {savedSearchId}";
			QueryRequest queryRequest = CreateQueryRequest(condition);

			RelativityObject savedSearchObject = _relativityObjectManager.Query(queryRequest).FirstOrDefault();
			return savedSearchObject != null ? ConvertRelativityObjectToSavedSearchDTO(savedSearchObject) : null;
		}

		public IEnumerable<SavedSearchDTO> RetrievePublicSavedSearches()
		{
			return GetSavedSearchesDto().Where(item => item.IsPublic);
		}

		public SavedSearchQueryResult RetrievePublicSavedSearches(SavedSearchQueryRequest request)
		{
			string condition = $"NOT '{_OWNER_FIELD}' ISSET";
			if (!string.IsNullOrEmpty(request.SearchTerm))
			{
				string nameCondition = $"('{_NAME_FIELD}' LIKE '%{request.SearchTerm}%')";
				condition = $"({condition}) AND ({nameCondition})";
			}

			QueryRequest queryRequest = CreateQueryRequest(condition);
			int start = (request.Page - 1) * request.PageSize;

			ResultSet<RelativityObject> result = _relativityObjectManager.Query(queryRequest, start, request.PageSize);
			List<SavedSearchDTO> mappedResult = result.Items.Select(ConvertRelativityObjectToSavedSearchDTO).ToList();

			return new SavedSearchQueryResult(request, result.TotalCount, mappedResult);
		}

		private IEnumerable<SavedSearchDTO> GetSavedSearchesDto(string condition = null)
		{
			QueryRequest query = CreateQueryRequest(condition);

			ArtifactDTO[] results = this.RetrieveAllArtifactsAsync(query).GetResultsWithoutContextSync();

			IEnumerable<SavedSearchDTO> savedSearches = results.Select(ConvertArtifactDTOToSavedSearchDTO);

			return savedSearches;
		}

		private static QueryRequest CreateQueryRequest(string condition)
		{
			return new QueryRequest
			{
				ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Search },
				Condition = condition,
				Fields = new[] { new FieldRef { Name = "Name" }, new FieldRef { Name = "Owner" } }
			};
		}

		private SavedSearchDTO ConvertArtifactDTOToSavedSearchDTO(ArtifactDTO item)
		{
			return new SavedSearchDTO
			{
				ArtifactId = item.ArtifactId,
				Name = item.Fields.FirstOrDefault(field => field.Name == _NAME_FIELD)?.Value as string,
				Owner = item.Fields.FirstOrDefault(field => field.Name == _OWNER_FIELD)?.Value as string
			};
		}

		private SavedSearchDTO ConvertRelativityObjectToSavedSearchDTO(RelativityObject item)
		{
			return new SavedSearchDTO
			{
				ArtifactId = item.ArtifactID,
				ParentContainerId = item.ParentObject.ArtifactID,
				Name = item.FieldValues.FirstOrDefault(x => x.Field.Name == _NAME_FIELD)?.Value as string,
				Owner = item.FieldValues.FirstOrDefault(x => x.Field.Name == _OWNER_FIELD)?.Value as string
			};
		}
	}
}
