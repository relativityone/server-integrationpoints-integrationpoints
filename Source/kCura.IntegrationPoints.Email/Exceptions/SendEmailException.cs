using System;

namespace kCura.IntegrationPoints.Email.Exceptions
{
    [Serializable]
    public class SendEmailException : Exception
    {
        public SendEmailException(string message) : base(message)
        {
        }

        protected SendEmailException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
        ) : base(info, context)
        {
        }
    }
}
