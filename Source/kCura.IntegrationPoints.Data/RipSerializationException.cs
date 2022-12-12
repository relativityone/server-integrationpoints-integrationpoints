using System;

namespace kCura.IntegrationPoints.Data
{
    public class RipSerializationException : Exception
    {
        public string Value { get; }

        public RipSerializationException(string message, string value, Exception innerException)
            : base(message, innerException)
        {
            Value = value;
        }
    }
}
