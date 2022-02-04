using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class ProductionHelper : WorkspaceHelperBase
    {
        private readonly SavedSearchHelper _savedSearchHelper;

        public ProductionHelper(WorkspaceTest workspace) : base(workspace)
        {
            _savedSearchHelper = Workspace.Helpers.SavedSearchHelper;
        }

        public IList<ProductionTest> GetAllProductions()
        {
            return Workspace.Productions;
        }

        public ProductionTest GetProduction(int productionId)
        {
            ProductionTest production = Workspace.Productions.First(x => x.ArtifactId == productionId);

            if (production == null)
            {
                throw new ArtifactNotFoundException($"Production with id {production.ArtifactId} not found");
            }

            return production;
        }

        public ProductionTest GetProductionBySearchCriteria(SearchCriteria searchCriteria)
        {
            SavedSearchTest savedSearch = _savedSearchHelper.GetSavedSearchBySearchCriteria(searchCriteria);
            ProductionTest production = GetProductionWithSavedSearchId(savedSearch.ArtifactId);
            return production;
        }

        private ProductionTest GetProductionWithSavedSearchId(int savedSearchId)
        {
            ProductionTest production = Workspace.Productions.First(x => x.SavedSearchId == savedSearchId);

            if (production == null)
            {
                throw new ArtifactNotFoundException($"Production with savedSearch id {savedSearchId} not found");
            }

            return production;
        }
    }
}
