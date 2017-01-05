using System;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class LdapController : ApiController
	{
		private readonly IEncryptionManager _manager;
		private readonly IHelper _helper;
		private readonly ISerializer _serializer;

		public LdapController(IEncryptionManager manager, IHelper helper, ISerializer serializer)
		{
			_manager = manager;
			_helper = helper;
			_serializer = serializer;
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
				JsonConvert.DeserializeObject<LDAPSettings>(message);
				decryptedText = message;
			}
			catch (Exception)
			{
				//already taken care of so we can just decrypt
				try { message = JsonConvert.DeserializeObject<string>(message); }
				catch (Exception) {/*just a regular string so we pass and decrypt it.*/ }
				decryptedText = _manager.Decrypt(message);
			}
			return decryptedText;
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve LDAP provider settings.")]
		public IHttpActionResult GetViewFields([FromBody] object data)
		{
			LDAPProvider.LDAPProvider provider = new LDAPProvider.LDAPProvider(_manager, _helper);
			LDAPSettings settings = provider.GetSettings(data.ToString());

			LdapProviderSummaryPageSettingsModel ldapSummarySettingsModel = new LdapProviderSummaryPageSettingsModel(settings);
			string serializedModel = _serializer.Serialize(ldapSummarySettingsModel);
			return Ok(serializedModel);
		}

	}
}
