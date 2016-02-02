using System;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Exceptions
{
	[Serializable]
	public class ReadDataFromRelativityException : Exception
	{
		public ReadDataFromRelativityException() : base()
		{
		}

		public ReadDataFromRelativityException(string message) : base(message)
		{
		}

		public ReadDataFromRelativityException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public ReadDataFromRelativityException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}