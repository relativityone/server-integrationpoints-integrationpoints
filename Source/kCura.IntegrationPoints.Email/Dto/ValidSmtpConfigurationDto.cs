namespace kCura.IntegrationPoints.Email.Dto
{
    internal class ValidSmtpConfigurationDto
    {
        public SmtpClientSettings SmtpClientSettings { get; }

        public ValidEmailAddress EmailFromAddres { get; }

        public ValidSmtpConfigurationDto(SmtpClientSettings smtpClientSettings, ValidEmailAddress emailFromAddres)
        {
            SmtpClientSettings = smtpClientSettings;
            EmailFromAddres = emailFromAddres;
        }
    }
}
