using System;
using System.Runtime.Serialization;

namespace Relativity.Sync.Tests.Performance.ARM
{
	[Serializable]
	public class ArmHelperException : Exception
	{
		public ArmHelperException() { }
		public ArmHelperException(string message) : base(message) { }
		public ArmHelperException(string message, Exception innerException) : base(message, innerException) { }
		protected ArmHelperException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
	}
}
