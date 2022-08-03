using System.Net;
using System.Net.Mail;
using kCura.IntegrationPoints.Email.Dto;

namespace kCura.IntegrationPoints.Email
{
    internal class SmtpClientFactory : ISmtpClientFactory
    {
        public SmtpClient Create(SmtpClientSettings settings)
        {
            return new SmtpClient(settings.Domain, settings.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(settings.UserName, settings.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = settings.UseSSL
            };
        }
    }
}
