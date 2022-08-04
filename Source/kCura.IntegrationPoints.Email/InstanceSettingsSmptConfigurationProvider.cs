using kCura.IntegrationPoints.Email.Dto;
using LanguageExt;
using Relativity.API;
using System;
using static LanguageExt.Prelude;

namespace kCura.IntegrationPoints.Email
{
    internal class InstanceSettingsSmptConfigurationProvider : ISmtpConfigurationProvider
    {
        private const string _KCURA_NOTIFICATION_CONFIG_SECTION = "kCura.Notification";
        private const string _SMTP_SERVER_SETTING_NAME = "SMTPServer";
        private const string _SMTP_PASSWORD_SETTING_NAME = "SMTPPassword";
        private const string _SMTP_SSL_IS_REQUIRED_SETTING_NAME = "SMTPSSLisRequired";
        private const string _SMTP_PORT_SETTING_NAME = "SMTPPort";
        private const string _SMTP_USER_NAME_SETTING_NAME = "SMTPUserName";
        private const string _SMTP_EMAIL_FROM_SETTING_NAME = "EmailFrom";

        private readonly IInstanceSettingsBundle _instanceSettingsBundle;
        private readonly IAPILog _logger;

        public InstanceSettingsSmptConfigurationProvider(
            IAPILog logger,
            IInstanceSettingsBundle instanceSettingsBundle)
        {
            _logger = logger.ForContext<InstanceSettingsSmptConfigurationProvider>();
            _instanceSettingsBundle = instanceSettingsBundle;
        }

        public Option<SmtpConfigurationDto> GetConfiguration()
        {
            try
            {
                return new SmtpConfigurationDto
                (
                    domain: GetNotificationConfigString(_SMTP_SERVER_SETTING_NAME),
                    password: GetNotificationConfigString(_SMTP_PASSWORD_SETTING_NAME),
                    useSsl: GetNotificationConfigBool(_SMTP_SSL_IS_REQUIRED_SETTING_NAME),
                    port: GetNotificationConfigUnsignedInt(_SMTP_PORT_SETTING_NAME),
                    userName: GetNotificationConfigString(_SMTP_USER_NAME_SETTING_NAME),
                    emailFromAddress: GetNotificationConfigString(_SMTP_EMAIL_FROM_SETTING_NAME)
                );
            }
            catch (Exception e)
            {
                LogReadingRelativityConfigurationError(e);
                return None;
            }
        }

        private string GetNotificationConfigString(string name)
        {
            return _instanceSettingsBundle.GetString(_KCURA_NOTIFICATION_CONFIG_SECTION, name);
        }

        private uint? GetNotificationConfigUnsignedInt(string name)
        {
            return _instanceSettingsBundle.GetUInt(_KCURA_NOTIFICATION_CONFIG_SECTION, name);
        }

        private bool? GetNotificationConfigBool(string name)
        {
            return _instanceSettingsBundle.GetBool(_KCURA_NOTIFICATION_CONFIG_SECTION, name);
        }

        private void LogReadingRelativityConfigurationError(Exception e)
        {
            _logger.LogError(e, "Failed to read SMTP configuration from instance settings.");
        }
    }
}