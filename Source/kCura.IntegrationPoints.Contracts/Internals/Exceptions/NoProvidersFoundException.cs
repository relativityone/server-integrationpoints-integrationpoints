using System;

namespace kCura.IntegrationPoints.Contracts.Internals.Exceptions
{
	[Serializable]
	internal class NoProvidersFoundException : Exception
	{
		public Guid Identifier { get; }

		public NoProvidersFoundException(Guid identifier): base()
		{
			Identifier = identifier;
		}

		public NoProvidersFoundException(string message): base(message)
		{
		}

		public NoProvidersFoundException(string message, System.Exception innerException): base(message, innerException)
		{
		}

		protected NoProvidersFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context): base(info, context)
		{
		}
	}
}
