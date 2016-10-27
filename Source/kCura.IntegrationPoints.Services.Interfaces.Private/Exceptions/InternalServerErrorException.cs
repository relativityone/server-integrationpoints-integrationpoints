using System;
using System.Net;
using System.Runtime.Serialization;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Services.Interfaces.Private.Exceptions
{
	[Serializable]
	[FaultCode(HttpStatusCode.InternalServerError)]
	public class InternalServerErrorException : ServiceException
	{
		public InternalServerErrorException()
		{
		}

		public InternalServerErrorException(string message) : base(message)
		{
		}

		public InternalServerErrorException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InternalServerErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}