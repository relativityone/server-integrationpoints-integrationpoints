using System.Collections.Generic;
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
        private readonly IIntegrationPointService _integrationPointService;

        public FieldMapController(IIntegrationPointService integrationPointService, ICPHelper helper)
        {
            _integrationPointService = integrationPointService;
            _apiLog = helper.GetLoggerFactory().GetLogger().ForContext<FieldMapController>();
        }

        [LogApiExceptionFilter(Message = "Unable to retrieve fields mapping information.")]
        public HttpResponseMessage Get(int id)
        {
            _apiLog.LogInformation("Retriving field mapping for Relativity Provider");

            // we need this hack because frontend calls this endpoint on step3 initialization,
            // whether it is editing or creating new integration point.
            List<FieldMap> fieldsMaps = id > 0
                ? _integrationPointService.GetFieldMap(id)
                : new List<FieldMap>();
            fieldsMaps.RemoveAll(
                fieldMap =>
                    fieldMap.FieldMapType == FieldMapTypeEnum.FolderPathInformation &&
                    string.IsNullOrEmpty(fieldMap.DestinationField.FieldIdentifier));

            return Request.CreateResponse(HttpStatusCode.OK, fieldsMaps, Configuration.Formatters.JsonFormatter);
        }
    }
}
