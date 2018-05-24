namespace kCura.IntegrationPoints.Domain.Models
{
    public class ValidationMessage
    {
        public ValidationMessage(string shortMessage) : this(string.Empty, shortMessage)
        {
        }

        public ValidationMessage(string errorCode, string shortMessage)
        {
            ErrorCode = errorCode;
            ShortMessage = shortMessage;
        }

        public string ErrorCode { get; set; }

        public string ShortMessage { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(ErrorCode) ? ShortMessage : $"{ErrorCode} {ShortMessage}";
        }
    }
}
