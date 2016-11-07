using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;
using Relativity.Services.FieldMapping;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class FieldCatalogController : ApiController
    {
        private readonly IHelper _helper;
   
        public FieldCatalogController(IHelper helper)
        {
            _helper = helper;
        }

        [LogApiExceptionFilter(Message = "Unable to retrieve field catalog information.")]
        public HttpResponseMessage Get(int id)
        {
            using (IFieldMapping proxy = _helper.GetServicesManager().CreateProxy<IFieldMapping>(ExecutionIdentity.System))
            {           
                ExternalMapping[] fieldsMap = proxy.GetAllMappedFieldsAsync(id, new Guid[0], 0).Result;
                return Request.CreateResponse(HttpStatusCode.OK, fieldsMap, Configuration.Formatters.JsonFormatter);
            }
        }
    }
}
