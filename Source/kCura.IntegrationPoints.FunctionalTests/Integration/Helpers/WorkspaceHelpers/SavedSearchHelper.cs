using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class SavedSearchHelper : WorkspaceHelperBase
    {
        public SavedSearchHelper(WorkspaceTest workspace) : base(workspace)
        {
        }

        public IList<SavedSearchTest> GetAllSavedSearches()
        {
            return Workspace.SavedSearches;
        }

        public SavedSearchTest GetSavedSearch(int savedSearchId)
        {
            SavedSearchTest savedSearch = Workspace.SavedSearches.First(x => x.ArtifactId == savedSearchId);

            if (savedSearch == null)
            {
                throw new ArtifactNotFoundException($"SavedSearch with id {savedSearch.ArtifactId} not found");
            }

            return savedSearch;
        }

        public SavedSearchTest GetSavedSearchBySearchCriteria(SearchCriteria searchCriteria)
        {
            SavedSearchTest savedSearch = Workspace.SavedSearches.First(x =>
                    x.SearchCriteria.HasImages == searchCriteria.HasImages &&
                    x.SearchCriteria.HasFields == searchCriteria.HasFields &&
                    x.SearchCriteria.HasNatives == searchCriteria.HasNatives);

            if (savedSearch == null)
            {
                throw new ArtifactNotFoundException($"SavedSearch with searchCriteria not found");
            }

            return savedSearch;
        }
    }
}
