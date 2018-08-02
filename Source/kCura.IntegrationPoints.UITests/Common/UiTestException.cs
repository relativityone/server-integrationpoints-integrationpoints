using System;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.UITests.Common
{
	[Serializable]
	public class UiTestException : Exception
	{
		public UiTestException()
		{
		}

		public UiTestException(string message) : base(message)
		{
		}

		public UiTestException(string message, Exception inner) : base(message, inner)
		{
		}

		protected UiTestException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}