using System;
using System.Net;
using System.Runtime.Serialization;
using Relativity.Services.Exceptions;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents an exception thrown when a user has insufficient permissions.
    /// </summary>
    [Serializable]
    [FaultCode(HttpStatusCode.Forbidden)]
    public class InsufficientPermissionException : ServiceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientPermissionException"/> class.
        /// </summary>
        public InsufficientPermissionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientPermissionException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InsufficientPermissionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientPermissionException"/> class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InsufficientPermissionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientPermissionException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected InsufficientPermissionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
