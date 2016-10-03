using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Extensions;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class SavedSearchesTreeController : ApiController
    {
        private readonly ISavedSearchesTreeService _savedSearchesService;
        private readonly IErrorRepository _errorRepository;

        public SavedSearchesTreeController(ISavedSearchesTreeService savedSearchesService, WebClientFactory webClientFactory, IRepositoryFactory repositoryFactory)
        {
            _savedSearchesService = savedSearchesService;
            _errorRepository = repositoryFactory.GetErrorRepository();
        }

        [HttpGet]
        public HttpResponseMessage Get(int workspaceArtifactId)
        {
            try
            {
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