using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Email;

namespace kCura.IntegrationPoints.Agent
{
	public class RelativityConfigurationFactory
	{
		public EmailConfiguration GetConfiguration()
		{
			return new EmailConfiguration
			{
				// TODO: This has to come back -- biedrzycki: Jan 26, 2016
//				Domain = kCura.Apps.Common.Config.Sections.NotificationConfig.SMTPServer,
//				Password = kCura.Apps.Common.Config.Sections.NotificationConfig.SMTPPassword,
//				Port = kCura.Apps.Common.Config.Sections.NotificationConfig.SMTPPort,
//				UserName = kCura.Apps.Common.Config.Sections.NotificationConfig.SMTPUserName,
//				UseSSL = kCura.Apps.Common.Config.Sections.NotificationConfig.SMTPSSLisRequired
			};

		}
	}
}
