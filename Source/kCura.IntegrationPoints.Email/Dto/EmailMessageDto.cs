namespace kCura.IntegrationPoints.Email.Dto
{
    public class EmailMessageDto
    {
        public string Subject { get; }

        public string Body { get; }

        public string ToAddress { get; }

        public EmailMessageDto(
            string subject,
            string body,
            string toAddress)
        {
            Subject = subject;
            Body = body;
            ToAddress = toAddress;
        }
    }
}
