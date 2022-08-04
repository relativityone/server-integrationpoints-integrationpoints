using System;
using System.Runtime.Serialization;
using kCura.IntegrationPoints.Domain.Exceptions;

namespace kCura.IntegrationPoints.Core.Services.Exporter.Sanitization
{
    /// <summary>
    /// Exception thrown when the data returned from the Export API is not in a form recognized by RIP old sync flow internal sanitizers.
    /// This would likely be due to a change in contract with the Export API.
    /// </summary>
    [Serializable]
    public sealed class InvalidExportFieldValueException : IntegrationPointsException
    {
        /// <inheritdoc />
        public InvalidExportFieldValueException() { }

        /// <inheritdoc />
        public InvalidExportFieldValueException(string message) : base(MessageTemplate(message)) { }

        /// <inheritdoc />
        public InvalidExportFieldValueException(string message, Exception innerException) : base(MessageTemplate(message), innerException) { }

        private InvalidExportFieldValueException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

        private static string MessageTemplate(string message)
        {
            return $"Unable to parse data from Relativity Export API: {message}";
        }
    }
}
