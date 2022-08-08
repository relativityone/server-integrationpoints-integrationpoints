using kCura.IntegrationPoints.Email.Dto;

namespace kCura.IntegrationPoints.Email
{
    public interface IEmailSender
    {
        void Send(EmailMessageDto message);
    }
}
