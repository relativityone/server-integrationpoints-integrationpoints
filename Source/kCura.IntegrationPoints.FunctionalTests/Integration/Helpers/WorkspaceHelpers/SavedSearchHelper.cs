using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class SavedSearchHelper : WorkspaceHelperBase
    {
        public SavedSearchHelper(WorkspaceFake workspace) : base(workspace)
        {
        }

        public IList<SavedSearchFake> GetAllSavedSearches()
        {
            return Workspace.SavedSearches;
        }

        public SavedSearchFake GetSavedSearch(int savedSearchId)
        {
            SavedSearchFake savedSearch = Workspace.SavedSearches.First(x => x.ArtifactId == savedSearchId);

            if (savedSearch == null)
            {
                throw new ArtifactNotFoundException($"SavedSearch with id {savedSearch.ArtifactId} not found");
            }

            return savedSearch;
        }

        public SavedSearchFake GetSavedSearchBySearchCriteria(SearchCriteria searchCriteria)
        {
            SavedSearchFake savedSearch = Workspace.SavedSearches.First(x =>
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
