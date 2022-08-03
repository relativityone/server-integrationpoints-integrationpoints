using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Core.Services
{
    public class SavedSearchesTreeService : ISavedSearchesTreeService
    {
        private readonly ISearchContainerManager _searchContainerManager;
        private readonly ISavedSearchesTreeCreator _treeCreator;
        private readonly IRepositoryFactory _repositoryFactory;

        public SavedSearchesTreeService(IHelper helper, ISavedSearchesTreeCreator treeCreator, IRepositoryFactory repositoryFactory)
        {
            _searchContainerManager = helper.GetServicesManager().CreateProxy<ISearchContainerManager>(ExecutionIdentity.CurrentUser);
            _treeCreator = treeCreator;
            _repositoryFactory = repositoryFactory;
        }

        public Task<JsTreeItemDTO> GetSavedSearchesTreeAsync(int workspaceArtifactId, int? nodeId = null, int? savedSearchId = null)
        {
            if (!nodeId.HasValue && !savedSearchId.HasValue)
            {
                return GetSavedSearchesTreeForRootAndDirectChildren(workspaceArtifactId);
            }
            if (!nodeId.HasValue)
            {
                return GetSavedSearchesTreeWithVisibleSavedSearch(workspaceArtifactId, savedSearchId.Value);
            }
            return GetSavedSearchesTreeForDirectChildrenOfNode(workspaceArtifactId, nodeId.Value);
        }

        private async Task<JsTreeItemDTO> GetSavedSearchesTreeForRootAndDirectChildren(int workspaceArtifactId)
        {
            SearchContainerItemCollection searchContainterCollection = await _searchContainerManager.GetSearchContainerTreeAsync(workspaceArtifactId, Enumerable.Empty<int>().ToList()).ConfigureAwait(false);
            return _treeCreator.Create(searchContainterCollection.SearchContainerItems,
                FilterAvailableSavedSearches(searchContainterCollection.SavedSearchContainerItems));
        }

        private async Task<JsTreeItemDTO> GetSavedSearchesTreeWithVisibleSavedSearch(int workspaceArtifactId, int savedSearchId)
        {
            List<int> allAncestors = await GetAllAncestorIdsForSavedSearch(workspaceArtifactId, savedSearchId).ConfigureAwait(false);
            SearchContainerItemCollection searchContainterCollection = await _searchContainerManager.GetSearchContainerTreeAsync(workspaceArtifactId, allAncestors).ConfigureAwait(false);

            return _treeCreator.Create(searchContainterCollection.SearchContainerItems,
                FilterAvailableSavedSearches(searchContainterCollection.SavedSearchContainerItems));
        }

        private async Task<JsTreeItemDTO> GetSavedSearchesTreeForDirectChildrenOfNode(int workspaceArtifactId, int nodeId)
        {
            var rootContainerReference = new SearchContainerRef(nodeId);
            Task<SearchContainerItemCollection> childItemsTask = _searchContainerManager.GetSearchContainerItemsAsync(workspaceArtifactId, rootContainerReference);
            SearchContainer rootContainer = await _searchContainerManager.ReadSingleAsync(workspaceArtifactId, rootContainerReference.ArtifactID).ConfigureAwait(false);
            SearchContainerItemCollection childItems = await childItemsTask.ConfigureAwait(false);

            return _treeCreator.CreateTreeForNodeAndDirectChildren(rootContainer, childItems.SearchContainerItems,
                FilterAvailableSavedSearches(childItems.SavedSearchContainerItems));
        }

        private async Task<List<int>> GetAllAncestorIdsForSavedSearch(int workspaceArtifactId, int savedSearchId)
        {
            ISavedSearchQueryRepository savedSearchQueryRepository = _repositoryFactory.GetSavedSearchQueryRepository(workspaceArtifactId);
            int nodeId = savedSearchQueryRepository.RetrieveSavedSearch(savedSearchId).ParentContainerId;

            var allAncestors = new List<int>();
            SearchContainer currentNode = await _searchContainerManager.ReadSingleAsync(workspaceArtifactId, nodeId).ConfigureAwait(false);
            while (currentNode != null)
            {
                allAncestors.Add(currentNode.ArtifactID);

                SearchContainer parentNode;
                try
                {
                    parentNode = await _searchContainerManager.ReadSingleAsync(workspaceArtifactId, currentNode.ParentSearchContainer.ArtifactID).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    parentNode = null; // current node is root - no parent
                }
                currentNode = parentNode;
            }

            return allAncestors;
        }

        private IEnumerable<SavedSearchContainerItem> FilterAvailableSavedSearches(
            IEnumerable<SavedSearchContainerItem> savedSearches)
        {
            return savedSearches.Where(i => i.Secured == false);
        }
    }
}