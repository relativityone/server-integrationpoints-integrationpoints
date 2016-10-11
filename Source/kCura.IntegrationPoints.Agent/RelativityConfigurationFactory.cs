using System;
using kCura.Apps.Common.Config.Sections;
using kCura.IntegrationPoints.Email;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent
{
	namespace kCura.IntegrationPoints.Agent
	{
		public class RelativityConfigurationFactory : IRelativityConfigurationFactory
		{
			private readonly IAPILog _logger;

			public RelativityConfigurationFactory(IHelper helper)
			{
				_logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativityConfigurationFactory>();
			}

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
				catch (Exception e)
				{
					LogCreatingRelativityConfigurationError(e);
					// DO NOT THROW EXCEPTION HERE
				}
				return config;
			}

			#region Logging

			private void LogCreatingRelativityConfigurationError(Exception e)
			{
				_logger.LogError(e, "Failed to create Relativity configuration.");
			}

			#endregion
		}
	}
}