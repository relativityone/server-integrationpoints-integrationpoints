using System.Net.Mail;

namespace kCura.IntegrationPoints.Email
{
	public interface ISMTPClientFactory
	{
		SmtpClient GetClient(EmailConfiguration configuration);
	}
}