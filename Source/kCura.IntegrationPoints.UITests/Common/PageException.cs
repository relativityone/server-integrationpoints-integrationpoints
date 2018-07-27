using System;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.UITests.Common
{
	[Serializable]
	public class PageException : UiTestException
	{
		public PageException()
		{
		}

		public PageException(string message) : base(message)
		{
		}

		public PageException(string message, Exception inner) : base(message, inner)
		{
		}

		protected PageException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}