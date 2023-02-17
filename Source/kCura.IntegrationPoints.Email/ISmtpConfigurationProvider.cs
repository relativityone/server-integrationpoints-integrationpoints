using kCura.IntegrationPoints.Email.Dto;
using LanguageExt;

namespace kCura.IntegrationPoints.Email
{
    /// <summary>
    /// Responsible for creating Relativity configurations.
    /// </summary>
    internal interface ISmtpConfigurationProvider
    {
        /// <summary>
        /// Retrieves an <see cref="Option{SmtpConfigurationDto}"/> instance.
        /// </summary>
        /// <returns>An instance of the <see cref="Option{SmtpConfigurationDto}"/> struct.</returns>
        Option<SmtpConfigurationDto> GetConfiguration();
    }
}
