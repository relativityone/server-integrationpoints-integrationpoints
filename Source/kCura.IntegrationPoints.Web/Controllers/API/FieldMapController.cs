using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class FieldMapController : ApiController
    {
        private readonly IAPILog _apiLog;
        private readonly IIntegrationPointService _integrationPointReader;

        public FieldMapController(IIntegrationPointService integrationPointReader, ICPHelper helper)
        {
            _integrationPointReader = integrationPointReader;
            _apiLog = helper.GetLoggerFactory().GetLogger().ForContext<FieldMapController>();
        }

        [LogApiExceptionFilter(Message = "Unable to retrieve fields mapping information.")]
        public HttpResponseMessage Get(int id)
        {
            _apiLog.LogInformation("Retriving field mapping for Relativity Provider");
            List<FieldMap> fieldsMaps = _integrationPointReader.GetFieldMap(id).ToList();
            fieldsMaps.RemoveAll(
                fieldMap =>
                    fieldMap.FieldMapType == FieldMapTypeEnum.FolderPathInformation &&
                    string.IsNullOrEmpty(fieldMap.DestinationField.FieldIdentifier));

            return Request.CreateResponse(HttpStatusCode.OK, fieldsMaps, Configuration.Formatters.JsonFormatter);
        }
    }
}
