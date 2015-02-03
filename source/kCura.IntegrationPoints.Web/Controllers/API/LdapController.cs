using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.LDAPProvider;
using Newtonsoft.Json;

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
		public IHttpActionResult Decrypt([FromBody] string message)
		{
			var decryptedText = string.Empty;
			try
			{
				var result = Newtonsoft.Json.JsonConvert.DeserializeObject<LDAPSettings>(message);
				decryptedText = message;
			}
			catch (Exception e)
			{
				//already taken care of so we can just decrypt
				message = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(message);
				decryptedText = _manager.Decrypt(message);
			}
			return Ok(decryptedText);
		}

	}
}
