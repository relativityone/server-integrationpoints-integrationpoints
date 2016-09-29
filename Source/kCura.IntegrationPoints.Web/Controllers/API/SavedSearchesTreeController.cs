using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Extensions;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class SavedSearchesTreeController : ApiController
    {
        private readonly ISavedSearchesTreeService _savedSearchesService;
        private readonly IErrorRepository _errorRepository;
        private readonly ISearchContainerManager _searchContainerManager;

        public SavedSearchesTreeController(ISavedSearchesTreeService savedSearchesService, WebClientFactory webClientFactory, IRepositoryFactory repositoryFactory)
        {
            _savedSearchesService = savedSearchesService;
            _errorRepository = repositoryFactory.GetErrorRepository();
            _searchContainerManager = webClientFactory.CreateServicesMgr().CreateProxy<ISearchContainerManager>(global::Relativity.API.ExecutionIdentity.CurrentUser);
        }

        [HttpGet]
        public HttpResponseMessage Get(int workspaceArtifactId)
        {
            try
            {
                /*
                var query = new global::Relativity.Services.Query();
                SearchContainerQueryResultSet searchContainersQuery = _searchContainerManager.QueryAsync(workspaceArtifactId, query).Result;

                List<int> searchContainersArtifactIds = searchContainersQuery.Results.Select(x => x.Artifact.ArtifactID).ToList();

                SearchContainerItemCollection collection = _searchContainerManager.GetSearchContainerTreeAsync(workspaceArtifactId, searchContainersArtifactIds).Result;

                var containers = collection.SearchContainerItems.Select(x =>
                {
                    return new TreeItemWithParentIdDTO
                    {
                        Id = x.SearchContainer.ArtifactID.ToString(),
                        ParentId = x.ParentContainer.ArtifactID.ToString(),
                        Text = x.SearchContainer.Name
                    };
                }).ToDictionary(x => x.Id);

                foreach (var container in containers.Values)
                {
                    TreeItemWithParentIdDTO node;
                    if (containers.TryGetValue(container.ParentId, out node))
                    {
                        node.Children.Add(container);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, collection);
                */
                var tree = _savedSearchesService.GetSavedSearchesTree(workspaceArtifactId);
                return Request.CreateResponse(HttpStatusCode.OK, tree);
            }
            catch (Exception exception)
            {
                this.HandleError(workspaceArtifactId, _errorRepository, exception, "Unable to retrieve the saved searches. Please contact the system administrator.");

                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}