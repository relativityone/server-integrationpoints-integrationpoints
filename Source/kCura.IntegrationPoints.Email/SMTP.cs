using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace kCura.IntegrationPoints.Email
{
	public class SMTP : ISendable
	{
		private readonly EmailConfiguration _configuration;

		public SMTP(EmailConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void Send(MailMessage message)
		{
			using (SmtpClient client = GetClient())
			{
				client.Send(message);
			}
		}

		private void Validate()
		{
			var exceptions = new List<Exception>();
			if (_configuration == null)
			{
				exceptions.Add(new Exception(Properties.Resources.Invalid_SMTP_Settings));
			}
			else
			{
				if (_configuration.Port < 0)
				{
					exceptions.Add(new Exception(Properties.Resources.SMTP_Port_Negative));
				}
				if (string.IsNullOrEmpty(_configuration.Domain))
				{
					exceptions.Add(new Exception(Properties.Resources.SMTP_Requires_SMTP_Domain));
				}
			}
			if (exceptions.Any())
			{
				throw new AggregateException(exceptions);
			}
		}

		private SmtpClient GetClient()
		{
			Validate();

			var client = new SmtpClient(_configuration.Domain, _configuration.Port)
			{
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(_configuration.UserName, _configuration.Password),
				DeliveryMethod = SmtpDeliveryMethod.Network,
				EnableSsl = _configuration.UseSSL
			};
			return client;
		}
	}
}
