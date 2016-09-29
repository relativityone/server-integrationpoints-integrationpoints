using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Services
{
    public class SavedSearchesTreeService : ISavedSearchesTreeService
    {
        private readonly ISearchContainerManager _searchContainerManager;
        private readonly ISavedSearchesTreeCreator _treeCreator;

        public SavedSearchesTreeService(IHelper helper, ISavedSearchesTreeCreator treeCreator)
        {
            _searchContainerManager = helper.GetServicesManager().CreateProxy<ISearchContainerManager>(ExecutionIdentity.CurrentUser);
            _treeCreator = treeCreator;
        }

        public JsTreeItemDTO GetSavedSearchesTree(int workspaceArtifactId)
        {
            var query = new global::Relativity.Services.Query();
            SearchContainerQueryResultSet searchContainerQueryResultSet = _searchContainerManager.QueryAsync(workspaceArtifactId, query).Result;

            List<int> searchContainersArtifactIds = searchContainerQueryResultSet.Results.Select(x => x.Artifact.ArtifactID).ToList();

            SearchContainerItemCollection searchContainterCollection = _searchContainerManager.GetSearchContainerTreeAsync(workspaceArtifactId, searchContainersArtifactIds).Result;

            JsTreeItemDTO tree = _treeCreator.Create(searchContainterCollection.SearchContainerItems, searchContainterCollection.SavedSearchContainerItems);

            return tree;
        }
    }
}