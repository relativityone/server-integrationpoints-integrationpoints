using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class IntegrationPointTypesController : ApiController
    {
        private readonly IIntegrationPointTypeService _integrationPointTypeService;

        public IntegrationPointTypesController(IIntegrationPointTypeService integrationPointTypeService)
        {
            _integrationPointTypeService = integrationPointTypeService;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve integration point types.")]
        public HttpResponseMessage Get()
        {
            IList<IntegrationPointType> types = _integrationPointTypeService.GetAllIntegrationPointTypes();
            IEnumerable<IntegrationPointTypeModel> result = types.Select(x => new IntegrationPointTypeModel
            {
                Name = x.Name,
                ArtifactId = x.ArtifactId,
                Identifier = x.Identifier
            });

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
