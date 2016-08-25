using kCura.IntegrationPoints.Email;
using System;
using kCura.Apps.Common.Config.Sections;

namespace kCura.IntegrationPoints.Agent
{
	namespace kCura.IntegrationPoints.Agent
	{
		public class RelativityConfigurationFactory : IRelativityConfigurationFactory
		{
			public EmailConfiguration GetConfiguration()
			{
				EmailConfiguration config = null;
				try
				{
					config = new EmailConfiguration
					{
						Domain = NotificationConfig.SMTPServer,
						Password = NotificationConfig.SMTPPassword,
						Port = NotificationConfig.SMTPPort,
						UserName = NotificationConfig.SMTPUserName,
						UseSSL = NotificationConfig.SMTPSSLisRequired
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