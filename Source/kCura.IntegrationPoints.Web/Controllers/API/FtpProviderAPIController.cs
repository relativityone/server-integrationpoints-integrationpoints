using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class FtpProviderAPIController : ApiController
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IDataProviderFactory _providerFactory;
        private readonly ISerializer _serializer;

        public FtpProviderAPIController(ISettingsManager settingsManager, IDataProviderFactory providerFactory, ISerializer serializer)
        {
            _settingsManager = settingsManager;
            _providerFactory = providerFactory;
            _serializer = serializer;
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve list of fields.")]
        public IHttpActionResult GetColumnList([FromBody] SynchronizerSettings synchronizerSettings)
        {
            IDataSourceProvider ftpProvider = _providerFactory.GetDataProvider(
                Guid.Parse(Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING),
                Guid.Parse(FtpProvider.Helpers.Constants.Guids.FtpProviderEventHandler));

            IEnumerable<FieldEntry> fields = ftpProvider.GetFields(new DataSourceProviderConfiguration(synchronizerSettings.Settings, synchronizerSettings.Credentials));
            return Ok(fields.ToList());
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to decrypt ftp settings.")]
        public HttpResponseMessage GetViewFields([FromBody] object data)
        {
            Settings settings = _settingsManager.DeserializeSettings(data.ToString());
            var model = new FtpProviderSummaryPageSettingsModel(settings);
            string serializedModel = _serializer.Serialize(model);
            return Request.CreateResponse(HttpStatusCode.OK, serializedModel);
        }
    }
}
