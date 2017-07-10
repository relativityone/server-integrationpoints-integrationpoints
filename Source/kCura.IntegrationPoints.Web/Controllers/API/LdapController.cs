using System;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class LdapController : ApiController
	{
	    private readonly ILDAPSettingsReader _settingsReader;
	    private readonly IEncryptionManager _manager;
	    private readonly ISerializer _serializer;

		public LdapController(ILDAPSettingsReader settingsReader, IEncryptionManager manager, ISerializer serializer)
		{
		    _settingsReader = settingsReader;
		    _manager = manager;
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
			string decryptedText = _settingsReader.DecryptSettings(message);
			return Ok(decryptedText);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve LDAP provider settings.")]
		public IHttpActionResult GetViewFields([FromBody] object data)
		{
			LDAPSettings settings = _settingsReader.GetSettings(data.ToString());

			var ldapSummarySettingsModel = new LdapProviderSummaryPageSettingsModel(settings);
			string serializedModel = _serializer.Serialize(ldapSummarySettingsModel);
			return Ok(serializedModel);
		}

	}
}
