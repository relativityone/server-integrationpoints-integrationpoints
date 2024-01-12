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
        public InsufficientPermissionException()
        {
        }

        public InsufficientPermissionException(string message) : base(message)
        {
        }

        public InsufficientPermissionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InsufficientPermissionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
