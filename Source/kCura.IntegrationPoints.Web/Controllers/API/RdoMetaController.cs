using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class RdoMetaController : ApiController
    {
        private readonly Core.Services.ObjectTypeService _service;

        public RdoMetaController(Core.Services.ObjectTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve information about parent RDO.")]
        public HttpResponseMessage Get(int id)
        {
            var hasParent = _service.HasParent(id);
            return Request.CreateResponse(HttpStatusCode.OK, new {hasParent = hasParent});
        }
    }
}
