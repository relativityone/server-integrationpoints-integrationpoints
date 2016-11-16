using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;
using Relativity.Services.FieldMapping;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class FieldCatalogController : ApiController
    {
        private readonly IFieldCatalogService _fieldCatalogService;

        public FieldCatalogController(IFieldCatalogService fieldCatalogService)
        {
            _fieldCatalogService = fieldCatalogService;
        }

        [LogApiExceptionFilter(Message = "Unable to retrieve field catalog information.")]
        public HttpResponseMessage Get(int id)
        {
            ExternalMapping[] fieldsMap = _fieldCatalogService.GetAllFieldCatalogMappings(id);
            return Request.CreateResponse(HttpStatusCode.OK, fieldsMap, Configuration.Formatters.JsonFormatter);           
        }
    }
}
