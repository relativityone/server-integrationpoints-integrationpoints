using System.Net.Mail;
using kCura.IntegrationPoints.Email.Dto;

namespace kCura.IntegrationPoints.Email
{
    internal interface ISmtpClientFactory
    {
        SmtpClient Create(SmtpClientSettings settings);
    }
}