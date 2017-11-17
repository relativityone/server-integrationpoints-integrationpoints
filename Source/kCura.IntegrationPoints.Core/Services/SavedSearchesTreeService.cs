using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Core.Service;
using Relativity.Services;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Relativity.Services.User;
using SearchProviderCondition = Relativity.Services.Search.SearchProviderCondition;

namespace kCura.IntegrationPoints.Core.Services
{
	public class SavedSearchesTreeService : ISavedSearchesTreeService
	{
		private readonly ISearchContainerManager _searchContainerManager;
		private readonly ISavedSearchesTreeCreator _treeCreator;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IHtmlSanitizerManager _htmlSanitizerManager;

		public SavedSearchesTreeService(IHelper helper, ISavedSearchesTreeCreator treeCreator, IRepositoryFactory repositoryFactory, IHtmlSanitizerManager htmlSanitizerManager)
		{
			_searchContainerManager = helper.GetServicesManager().CreateProxy<ISearchContainerManager>(ExecutionIdentity.CurrentUser);
			_treeCreator = treeCreator;
			_repositoryFactory = repositoryFactory;
		    _htmlSanitizerManager = htmlSanitizerManager;
		}

		public JsTreeItemDTO GetSavedSearchesTree(int workspaceArtifactId)
		{
		    List<Result<SearchContainer>> folders = _searchContainerManager.QueryAsync(workspaceArtifactId, new Query( )).ConfigureAwait(false).GetAwaiter().GetResult().Results;
            List<int> searchContainersArtifactIds = folders.Select(x => x.Artifact.ArtifactID).ToList();

			SearchContainerItemCollection searchContainterCollection = _searchContainerManager.GetSearchContainerTreeAsync(workspaceArtifactId, searchContainersArtifactIds).ConfigureAwait(false).GetAwaiter().GetResult();
			return _treeCreator.Create(searchContainterCollection.SearchContainerItems, searchContainterCollection.SavedSearchContainerItems.Where(i=> i.Secured == false));
		}

        /// <summary>
        /// Traverse the tree using Breadth First Search (BFS) and sanitize each node's text
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public JsTreeItemDTO SanitizeTree(JsTreeItemDTO tree)
	    {
	        if (tree != null)
	        {
	            var nodesToVisit = new List<JsTreeItemDTO>() { tree };
	            while (nodesToVisit.Count > 0)
	            {
                    JsTreeItemDTO currentNode = nodesToVisit.First();
                    SanitizeNode(currentNode);
                    nodesToVisit.RemoveAt(0);
                    foreach (var node in currentNode.Children)
                    {
                        nodesToVisit.Add(node);
                    }
	            }
	        }
	        return tree;
	    }

	    private void SanitizeNode(JsTreeItemDTO node)
	    {
	        if (node != null)
	        {
	            string rawText = node.Text;
	            SanitizeResult sanitizedResult = _htmlSanitizerManager.Sanitize(rawText);
	            string sanitizedText = sanitizedResult.CleanHTML;
	            if (!sanitizedResult.HasErrors && !string.IsNullOrWhiteSpace(sanitizedText))
	            {
	                node.Text = sanitizedText;
	            }
	            else
	            {
	                node.Text = "Default Search Name";
	            }
	        }
	    }

	}
}