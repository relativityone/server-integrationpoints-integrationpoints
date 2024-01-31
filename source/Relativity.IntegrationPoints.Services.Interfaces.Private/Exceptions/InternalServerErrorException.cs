using System;
using System.Net;
using System.Runtime.Serialization;
using Relativity.Services.Exceptions;

namespace Relativity.IntegrationPoints.Services
{
    [Serializable]
    [FaultCode(HttpStatusCode.InternalServerError)]
    public class InternalServerErrorException : ServiceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerErrorException"/> class.
        /// </summary>
        public InternalServerErrorException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerErrorException"/> class.
        /// </summary>
        /// <param name="message"></param>
        public InternalServerErrorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerErrorException"/> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public InternalServerErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerErrorException"/> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected InternalServerErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
