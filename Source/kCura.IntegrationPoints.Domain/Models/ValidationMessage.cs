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

        protected bool Equals(ValidationMessage other)
        {
            return string.Equals(ErrorCode, other.ErrorCode) && string.Equals(ShortMessage, other.ShortMessage);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((ValidationMessage) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ErrorCode != null ? ErrorCode.GetHashCode() : 0) * 397) ^ (ShortMessage != null ? ShortMessage.GetHashCode() : 0);
            }
        }
    }
}
