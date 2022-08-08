using System;

namespace kCura.IntegrationPoints.Domain.Logging
{
    public class CorrelationContextCreationException : Exception
    {
        public CorrelationContextCreationException()
        { }

        public CorrelationContextCreationException(string message) : base(CreateErrorMessage(message))
        {}

        public CorrelationContextCreationException(string message, Exception exception): base(CreateErrorMessage(message), exception)
        { }

        private static string CreateErrorMessage(string message)
        {
            return $"Exception while creating correlation context: {message}";
        }
    }
}
