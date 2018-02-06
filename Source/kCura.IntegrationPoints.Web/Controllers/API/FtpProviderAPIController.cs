using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class FtpProviderAPIController : ApiController
    {
        private readonly IEncryptionManager _securityManager;
        private readonly ISettingsManager _settingsManager;
        private readonly IDataProviderFactory _providerFactory;
        private readonly IHelper _helper;
	    private readonly ISerializer _serializer;

        public FtpProviderAPIController(IEncryptionManager securityManager, ISettingsManager settingsManager, IDataProviderFactory providerFactory, IHelper helper, ISerializer serializer)
        {
            _securityManager = securityManager;
            _settingsManager = settingsManager;
            _providerFactory = providerFactory;
	        _helper = helper;
	        _serializer = serializer;
        }

        [HttpPost]
		[LogApiExceptionFilter(Message = "Unable to Encrypt message.")]
		public IHttpActionResult Encrypt([FromBody] object message)
        {
            string encryptedText = string.Empty;
            if (message != null)
            {
                encryptedText = _securityManager.Encrypt(message.ToString());
            }
            return Ok(encryptedText);
        }

        [HttpPost]
		[LogApiExceptionFilter(Message = "Unable to Decrypt message.")]
		public IHttpActionResult Decrypt([FromBody] string message)
        {
            string decryptedText = _securityManager.Decrypt(message);
            return Ok(decryptedText);
        }

        [HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve list of fields.")]
		public IHttpActionResult GetColumnList([FromBody] object data)
        {
            string encryptedSettings = _securityManager.Encrypt(data.ToString());
            IDataSourceProvider ftpProvider = _providerFactory.GetDataProvider(Guid.Parse(Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING), Guid.Parse(FtpProvider.Helpers.Constants.Guids.FtpProviderEventHandler));
            IEnumerable<FieldEntry> fields = ftpProvider.GetFields(encryptedSettings);
            return Ok(fields.ToList());
        }

        [HttpPost]
		[LogApiExceptionFilter(Message = "Unable to decrypt ftp settings.")]
		public HttpResponseMessage GetViewFields([FromBody] object data)
        {
            Settings settings = _settingsManager.ConvertFromEncryptedString(data.ToString());
			var model = new FtpProviderSummaryPageSettingsModel(settings);
	        string serializedModel = _serializer.Serialize(model);
            return Request.CreateResponse(HttpStatusCode.OK, serializedModel);
        }
    }
}
