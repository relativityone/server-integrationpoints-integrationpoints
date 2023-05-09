using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class WorkspaceFieldController : ApiController
    {
        private readonly ISynchronizerFactory _appDomainRdoSynchronizerFactory;
        private readonly ISerializer _serializer;
        private readonly IAPILog _apiLog;

        public WorkspaceFieldController(ISynchronizerFactory appDomainRdoSynchronizerFactory, ISerializer serializer, ICPHelper helper)
        {
            _appDomainRdoSynchronizerFactory = appDomainRdoSynchronizerFactory;
            _serializer = serializer;
            _apiLog = helper.GetLoggerFactory().GetLogger().ForContext<WorkspaceFieldController>();
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve workspace fields.")]
        public HttpResponseMessage Post([FromBody] SynchronizerSettings settings)
        {
            _apiLog.LogInformation("Import Settings has been extracted successfully for workspace field retrieval process");

            IDataSynchronizer synchronizer = _appDomainRdoSynchronizerFactory.CreateSynchronizer(Guid.Empty, _serializer.Deserialize<DestinationConfiguration>(settings.Settings));
            List<FieldEntry> fields = synchronizer.GetFields(new DataSourceProviderConfiguration(settings.Settings, settings.Credentials)).ToList();

            List<ClassifiedFieldDTO> result = fields.Select(x => new ClassifiedFieldDTO
            {
                ClassificationLevel = ClassificationLevel.AutoMap,
                FieldIdentifier = x.FieldIdentifier,
                Name = x.ActualName,
                Type = x.Type,
                IsIdentifier = x.IsIdentifier,
                IsRequired = x.IsRequired
            }).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
        }
    }
}
