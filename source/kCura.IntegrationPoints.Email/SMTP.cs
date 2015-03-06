using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace kCura.IntegrationPoints.Email
{
	public class SMTP : ISendable
	{
		private SmtpClient _client;
		private readonly EmailConfiguration _configuration;

		public SMTP(EmailConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void Send(System.Net.Mail.MailMessage message)
		{
			var client = GetClient();
			client.Send(message);
		}

		#region Private Functions

		protected virtual void Validate()
		{
			var exceptions = new List<Exception>();
			if (_configuration.Port < 0)
			{
				exceptions.Add(new Exception(Properties.Resources.SMTP_Port_Negative));
			}
			if (string.IsNullOrEmpty(_configuration.UserName) || string.IsNullOrEmpty(_configuration.Password))
			{
				exceptions.Add(new Exception(Properties.Resources.SMTP_USERNAME_PASSWORD_INVALID));
			}

			if (exceptions.Any())
			{
				throw new AggregateException(exceptions);
			}
		}

		protected virtual SmtpClient GetClient()
		{
			Validate();

			var client = new SmtpClient(_configuration.Domain, _configuration.Port);
			client.UseDefaultCredentials = false;
			client.Credentials = new NetworkCredential(_configuration.UserName, _configuration.Password);
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.EnableSsl = _configuration.UseSSL;
			return client;
		}

		#endregion

	}
}
