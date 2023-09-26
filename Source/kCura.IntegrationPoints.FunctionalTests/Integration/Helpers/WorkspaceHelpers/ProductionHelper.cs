using System.Collections.Generic;
using System.Linq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.Exceptions;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
    public class ProductionHelper : WorkspaceHelperBase
    {
        private readonly SavedSearchHelper _savedSearchHelper;

        public ProductionHelper(WorkspaceFake workspace) : base(workspace)
        {
            _savedSearchHelper = Workspace.Helpers.SavedSearchHelper;
        }

        public IList<ProductionFake> GetAllProductions()
        {
            return Workspace.Productions;
        }

        public ProductionFake GetProduction(int productionId)
        {
            ProductionFake production = Workspace.Productions.First(x => x.ArtifactId == productionId);

            if (production == null)
            {
                throw new ArtifactNotFoundException($"Production with id {production.ArtifactId} not found");
            }

            return production;
        }

        public ProductionFake GetProductionBySearchCriteria(SearchCriteria searchCriteria)
        {
            SavedSearchFake savedSearch = _savedSearchHelper.GetSavedSearchBySearchCriteria(searchCriteria);
            ProductionFake production = GetProductionWithSavedSearchId(savedSearch.ArtifactId);
            return production;
        }

        private ProductionFake GetProductionWithSavedSearchId(int savedSearchId)
        {
            ProductionFake production = Workspace.Productions.First(x => x.SavedSearchId == savedSearchId);

            if (production == null)
            {
                throw new ArtifactNotFoundException($"Production with savedSearch id {savedSearchId} not found");
            }

            return production;
        }
    }
}
