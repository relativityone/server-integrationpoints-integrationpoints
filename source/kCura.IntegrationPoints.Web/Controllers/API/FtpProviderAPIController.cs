using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Security;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class FtpProviderAPIController : ApiController
    {
        private IEncryptionManager _securityManager;
        private ISettingsManager _settingsManager;
        private IProviderFactory _providerFactory;

        public FtpProviderAPIController(IEncryptionManager securityManager, ISettingsManager settingsManager, IProviderFactory providerFactory)
        {
            _securityManager = securityManager;
            _settingsManager = settingsManager;
            _providerFactory = providerFactory;
        }

        [HttpPost]
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
        public IHttpActionResult Decrypt([FromBody] string message)
        {
            string decryptedText = _securityManager.Decrypt(message);
            return Ok(decryptedText);
        }

        [HttpPost]
        public IHttpActionResult GetColumnList([FromBody] object data)
        {
	        List<FieldEntry> result = null;
	        try
	        {
				string encryptedSettings = _securityManager.Encrypt(data.ToString());
				IDataSourceProvider ftpProvider = _providerFactory.CreateProvider(Guid.Parse(FtpProvider.Helpers.Constants.Guids.FtpProviderEventHandler));
				IEnumerable<Contracts.Models.FieldEntry> fields = ftpProvider.GetFields(encryptedSettings);
				result = fields.ToList();
			}
	        catch (Exception ex)
	        {
		        return Content(HttpStatusCode.BadRequest, ex.Message);
			}
           

            return Ok(result);
        }

        [HttpPost]
        public HttpResponseMessage GetViewFields([FromBody] object data)
        {
            Settings settings = _settingsManager.ConvertFromEncryptedString(data.ToString());
            var model = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("host", settings.Host),
                new KeyValuePair<string, string>("port", settings.Port.ToString()),
                new KeyValuePair<string, string>("protocol", settings.Protocol),
                new KeyValuePair<string, string>("username", settings.Username ?? string.Empty),
                new KeyValuePair<string, string>("password", "******"),
                new KeyValuePair<string, string>("filename_prefix", settings.Filename_Prefix),
                new KeyValuePair<string, string>("timezone_offset", settings.Timezone_Offset.ToString())
            };
            return Request.CreateResponse(HttpStatusCode.OK, model);
        }
    }
}
