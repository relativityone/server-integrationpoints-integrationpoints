using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.DestinationTypes;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class DestinationTypeController : ApiController
    {
        private readonly IDestinationTypeFactory _factory;

        public DestinationTypeController(IDestinationTypeFactory factory)
        {
            _factory = factory;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve destination provider type info.")]
        public HttpResponseMessage Get()
        {
            List<DestinationType> list = _factory.GetDestinationTypes().ToList();
            return Request.CreateResponse(HttpStatusCode.OK, list);
        }
    }
}
