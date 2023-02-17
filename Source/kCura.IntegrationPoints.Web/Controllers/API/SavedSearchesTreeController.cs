using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class SavedSearchesTreeController : ApiController
    {
        private readonly ISavedSearchesTreeService _savedSearchesService;

        public SavedSearchesTreeController(ISavedSearchesTreeService savedSearchesService)
        {
            _savedSearchesService = savedSearchesService;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve saved searches list.")]
        public async Task<HttpResponseMessage> Get(int workspaceArtifactId, int? savedSearchContainerId = null, int? savedSearchId = null)
        {
            JsTreeItemDTO tree = await _savedSearchesService.GetSavedSearchesTreeAsync(workspaceArtifactId, savedSearchContainerId, savedSearchId).ConfigureAwait(true);
            return Request.CreateResponse(HttpStatusCode.OK, tree);
        }
    }
}
