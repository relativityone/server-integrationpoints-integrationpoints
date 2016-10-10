using System;
using System.Collections.Generic;
using System.Web.Http;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class LdapController : ApiController
    {
        private readonly IEncryptionManager _manager;
        public LdapController(IEncryptionManager manager)
        {
            _manager = manager;
        }

        [HttpPost]
		[LogApiExceptionFilter(Message = "Unable to Encrypt message data.")]
		public IHttpActionResult Encrypt([FromBody] object message)
        {
            var decryptedText = string.Empty;
            if (message != null)
            {
                decryptedText = _manager.Encrypt(message.ToString());
            }
            return Ok(decryptedText);
        }

        [HttpPost]
		[LogApiExceptionFilter(Message = "Unable to Decrypt message data.")]
		public IHttpActionResult Decrypt([FromBody] string message)
        {
            var decryptedText = this.GetSettings(message);
            return Ok(decryptedText);
        }

        private string GetSettings(string message)
        {
            var decryptedText = string.Empty;
            try
            {
                Newtonsoft.Json.JsonConvert.DeserializeObject<LDAPSettings>(message);
                decryptedText = message;
            }
            catch (Exception)
            {
                //already taken care of so we can just decrypt
                try { message = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(message); }
                catch (Exception) {/*just a regular string so we pass and decrypt it.*/ }
                decryptedText = _manager.Decrypt(message);
            }
            return decryptedText;
        }

        [HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve LDAP provider settings.")]
		public IHttpActionResult GetViewFields([FromBody] object data)
        {
            var provider = new LDAPProvider.LDAPProvider(_manager);
            LDAPSettings settings = provider.GetSettings(data.ToString());
            var result = new List<KeyValuePair<string, string>>();
            result.Add(new KeyValuePair<string, string>("Connection Path", settings.ConnectionPath));
            result.Add(new KeyValuePair<string, string>("Object Filter String", settings.Filter));
            result.Add(new KeyValuePair<string, string>("Authentication", settings.ConnectionAuthenticationType.ToString()));
            result.Add(new KeyValuePair<string, string>("Username", settings.UserName ?? string.Empty));
            result.Add(new KeyValuePair<string, string>("Password", "******"));
            result.Add(new KeyValuePair<string, string>("Import Nested Items", settings.ImportNested ? "Yes" : "No"));
            return Ok(result);
        }

    }
}
