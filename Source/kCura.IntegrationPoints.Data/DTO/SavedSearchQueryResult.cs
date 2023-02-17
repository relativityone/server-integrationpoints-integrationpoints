using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.DTO
{
    public class SavedSearchQueryResult
    {
        public SavedSearchQueryResult(SavedSearchQueryRequest searchRequest, int totalResults, List<SavedSearchDTO> savedSearches)
        {
            SearchRequest = searchRequest;
            TotalResults = totalResults;
            SavedSearches = savedSearches;
        }

        public SavedSearchQueryRequest SearchRequest { get; }

        public int TotalResults { get; }

        public List<SavedSearchDTO> SavedSearches { get; }

        public bool HasMoreResults => SearchRequest.Page * SearchRequest.PageSize < TotalResults;
    }
}
