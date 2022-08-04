using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Converters;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class SavedSearchQueryRepository : ISavedSearchQueryRepository
    {
        private readonly IRelativityObjectManager _relativityObjectManager;

        public SavedSearchQueryRepository(IRelativityObjectManager relativityObjectManager)
        {
            _relativityObjectManager = relativityObjectManager;
        }

        public SavedSearchDTO RetrieveSavedSearch(int savedSearchId)
        {
            string condition = $"'Artifact ID' == {savedSearchId}";
            QueryRequest queryRequest = CreateQueryRequest(condition);

            RelativityObject savedSearchObject = _relativityObjectManager.Query(queryRequest).FirstOrDefault();
            return savedSearchObject?.ToSavedSearchDTO();
        }

        public IEnumerable<SavedSearchDTO> RetrievePublicSavedSearches()
        {
            return GetSavedSearchesDtoAsync().GetAwaiter().GetResult().Where(item => item.IsPublic);
        }

        public SavedSearchQueryResult RetrievePublicSavedSearches(SavedSearchQueryRequest request)
        {
            string condition = $"NOT '{SavedSearchFieldsConstants.OWNER_FIELD}' ISSET";
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                string nameCondition = $"('{SavedSearchFieldsConstants.NAME_FIELD}' LIKE '%{request.SearchTerm}%')";
                condition = $"({condition}) AND ({nameCondition})";
            }

            QueryRequest queryRequest = CreateQueryRequest(condition);
            int start = (request.Page - 1) * request.PageSize;

            ResultSet<RelativityObject> result = _relativityObjectManager.Query(queryRequest, start, request.PageSize);
            List<SavedSearchDTO> mappedResult = result.Items.ToSavedSearchDTOs().ToList();

            return new SavedSearchQueryResult(request, result.TotalCount, mappedResult);
        }

        private async Task<IEnumerable<SavedSearchDTO>> GetSavedSearchesDtoAsync(string condition = null)
        {
            QueryRequest query = CreateQueryRequest(condition);

            IEnumerable<RelativityObject> results = await _relativityObjectManager.QueryAsync(query).ConfigureAwait(false);

            IEnumerable<SavedSearchDTO> savedSearches = results.ToSavedSearchDTOs();

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
    }
}
