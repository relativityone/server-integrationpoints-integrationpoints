using System.Collections.Generic;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface ISavedSearchQueryRepository
    {
        /// <summary>
        /// Retrieves the current Saved Search.
        /// </summary>
        /// <returns>Returns the current SavedSearchDTO.</returns>
        SavedSearchDTO RetrieveSavedSearch(int savedSearchId);

        /// <summary>
        /// Retrieves Public Saved Searches
        /// </summary>
        /// <returns>Returns the current SavedSearchDTO.</returns>
        IEnumerable<SavedSearchDTO> RetrievePublicSavedSearches();

        SavedSearchQueryResult RetrievePublicSavedSearches(SavedSearchQueryRequest request);
    }
}
