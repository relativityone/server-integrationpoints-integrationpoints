using kCura.IntegrationPoints.Email;
using System;

namespace kCura.IntegrationPoints.Agent
{
	namespace kCura.IntegrationPoints.Agent
	{
		public class RelativityConfigurationFactory
		{
			public EmailConfiguration GetConfiguration()
			{
				EmailConfiguration config = null;
				try
				{
					config = new EmailConfiguration
					{
						Domain = kCura.Apps.Common.Config.Sections.NotificationConfig.SMTPServer,
						Password = kCura.Apps.Common.Config.Sections.NotificationConfig.SMTPPassword,
						Port = kCura.Apps.Common.Config.Sections.NotificationConfig.SMTPPort,
						UserName = kCura.Apps.Common.Config.Sections.NotificationConfig.SMTPUserName,
						UseSSL = kCura.Apps.Common.Config.Sections.NotificationConfig.SMTPSSLisRequired
					};
				}
				catch (Exception)
				{
					// DO NOT THROW EXCEPTION HERE
				}
				return config;
			}
		}
	}
}