using System;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	[Serializable]
	public class InvalidSourceProviderException : Exception
	{
		public InvalidSourceProviderException()
		{

		}
		public InvalidSourceProviderException(string message)
			: base(message)
		{
		}
		public InvalidSourceProviderException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
