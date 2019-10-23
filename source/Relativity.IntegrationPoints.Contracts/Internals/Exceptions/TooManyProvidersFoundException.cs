using System;

namespace Relativity.IntegrationPoints.Contracts.Internals.Exceptions
{
	[Serializable]
	internal class TooManyProvidersFoundException : Exception
	{
		public int ProviderCount { get; }
		public Guid Identifier { get; }

		public TooManyProvidersFoundException(int providerCount, Guid identifier)
		{
			ProviderCount = providerCount;
			Identifier = identifier;
		}

		public TooManyProvidersFoundException()
		{
		}

		public TooManyProvidersFoundException(string message): base(message)
		{
		}

		public TooManyProvidersFoundException(string message, Exception innerException): base(message, innerException)
		{
		}

		protected TooManyProvidersFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context): base(info, context)
		{
		}
	}
}
