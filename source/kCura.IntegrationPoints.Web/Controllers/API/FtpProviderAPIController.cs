using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Security;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class FtpProviderAPIController : ApiController
    {
        private IEncryptionManager _securityManager;
        private ISettingsManager _settingsManager;
        public FtpProviderAPIController(IEncryptionManager securityManager, ISettingsManager settingsManager)
        {
            _securityManager = securityManager;
            _settingsManager = settingsManager;
        }

        [HttpPost]
        public IHttpActionResult Encrypt([FromBody] object message)
        {
            var decryptedText = string.Empty;
            if (message != null)
            {
                decryptedText = _securityManager.Encrypt(message.ToString());
            }
            return Ok(decryptedText);
        }

        [HttpPost]
        public IHttpActionResult Decrypt([FromBody] string message)
        {
            var decryptedText = _securityManager.Decrypt(message);
            return Ok(decryptedText);
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
