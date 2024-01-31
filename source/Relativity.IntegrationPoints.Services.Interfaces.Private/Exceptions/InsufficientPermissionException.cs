using System;
using System.Net;
using System.Runtime.Serialization;
using Relativity.Services.Exceptions;

namespace Relativity.IntegrationPoints.Services
{
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
        /// Initializes a new instance of the <see cref="InsufficientPermissionException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public InsufficientPermissionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientPermissionException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Another exception that is cause of the current exception.</param>
        public InsufficientPermissionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientPermissionException"/> class.
        /// </summary>
        /// <param name="info">Input parameter for SerializationInfo.</param>
        /// <param name="context">Input parameter for StreamingContext.</param>
        protected InsufficientPermissionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
