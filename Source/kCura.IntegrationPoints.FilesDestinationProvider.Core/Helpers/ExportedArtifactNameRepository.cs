using System.Linq;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;
using Relativity.Productions.Services;
using Relativity.Services.Search;
using Relativity.Services.View;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
    public class ExportedArtifactNameRepository : IExportedArtifactNameRepository
    {
        private readonly IServicesMgr _servicesMgr;

        public ExportedArtifactNameRepository(IServicesMgr servicesMgr)
        {
            _servicesMgr = servicesMgr;
        }

        public string GetViewName(int workspaceId, int viewId)
        {
            using (IViewManager viewManager = _servicesMgr.CreateProxy<IViewManager>(ExecutionIdentity.System))
            {
                View view = viewManager.ReadSingleAsync(workspaceId, viewId).GetAwaiter().GetResult();
                return view.Name;
            }
        }

        public string GetProductionName(int workspaceId, int productionId)
        {
            using(var productionManager = _servicesMgr.CreateProxy<IProductionManager>(ExecutionIdentity.System))
            {
                Production production = productionManager.ReadSingleAsync(workspaceId, productionId).GetAwaiter().GetResult();

                if(production == null)
                {
                    throw new IntegrationPointsException($"Production {productionId} was not found.");
                }

                return production.Name;
            }
        }

        public string GetSavedSearchName(int workspaceId, int savedSearchId)
        {
            var query = new GetSavedSearchQuery(_servicesMgr, workspaceId, savedSearchId);
            KeywordSearchQueryResultSet queryResult = query.ExecuteQuery();
            
            if (!queryResult.Success)
            {
                throw new IntegrationPointsException($"Error occured when querying for saved search Artifact ID: {savedSearchId}. Message: {queryResult.Message}");
            }

            if (!queryResult.Results.Any())
            {
                throw new IntegrationPointsException($"Cannot find saved search artifact ID: {savedSearchId}");
            }

            return queryResult.Results.Single().Artifact.Name;
        }
    }
}
