using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Email
{
	public class SMTPClientFactory : ISMTPClientFactory
	{
		public SmtpClient GetClient(EmailConfiguration configuration)
		{
			var client = new SmtpClient(configuration.Domain, configuration.Port)
			{
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(configuration.UserName, configuration.Password),
				DeliveryMethod = SmtpDeliveryMethod.Network,
				EnableSsl = configuration.UseSSL
			};
			return client;

		}
	}
}
