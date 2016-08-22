using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace kCura.IntegrationPoints.Email
{
	public interface ISendable
	{
		void Send(MailMessage message);
	}
}
