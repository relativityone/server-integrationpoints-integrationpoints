using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services.SourceTypes;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class SourceTypeController : ApiController
    {
        private readonly ISourceTypeFactory _factory;
        private readonly IObjectTypeRepository _objectTypeRepository;
        private readonly IAPILog _apiLog;

        public SourceTypeController(
            ISourceTypeFactory factory,
            IObjectTypeRepository objectTypeRepository,
            ICPHelper helper)
        {
            _factory = factory;
            _objectTypeRepository = objectTypeRepository;
            _apiLog = helper.GetLoggerFactory().GetLogger();
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve source provider list.")]
        public HttpResponseMessage Get()
        {
            _apiLog.LogInformation("Retriving Source Provider Types...");
            Dictionary<Guid, int> rdoTypesCache = _objectTypeRepository.GetRdoGuidToArtifactIdMap();
            List<SourceTypeModel> list = _factory.GetSourceTypes().Select(x => new SourceTypeModel
            {
                name = x.Name,
                id = x.ArtifactID,
                value = x.ID,
                url = x.SourceURL,
                Config = new SourceProviderConfigModel(x.Config, rdoTypesCache)
            }).ToList();

            _apiLog.LogInformation($"Retriving Source Provider Types completed. Found: {list.Count} types");

            return Request.CreateResponse(HttpStatusCode.OK, list);
        }
    }
}
