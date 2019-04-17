using System;
using System.Runtime.Serialization;

namespace kCura.IntegrationPoints.PerformanceTestingFramework.Helpers
{
	[Serializable]
	public class TestContextParametersHelperException : Exception
	{
		public TestContextParametersHelperException()
		{
		}

		public TestContextParametersHelperException(string message) : base(message)
		{
		}

		public TestContextParametersHelperException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected TestContextParametersHelperException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}