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
			private const string _SMTP_SETTINGS_SECTION = "kCura.Notification";
			private const string _SMTP_PASSWORD_SETTING_NAME = "SMTPPassword";

			private readonly IAPILog _logger;
			private readonly IInstanceSettingsBundle _instanceSettingsBundle;

			public RelativityConfigurationFactory(IHelper helper)
			{
				_logger = helper.GetLoggerFactory().GetLogger().ForContext<RelativityConfigurationFactory>();
				_instanceSettingsBundle = helper.GetInstanceSettingBundle();
			}

			public EmailConfiguration GetConfiguration()
			{
				EmailConfiguration config = null;
				try
				{
					config = new EmailConfiguration
					{
						Domain = NotificationConfig.SMTPServer,
						Password = _instanceSettingsBundle.GetString(_SMTP_SETTINGS_SECTION, _SMTP_PASSWORD_SETTING_NAME),
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