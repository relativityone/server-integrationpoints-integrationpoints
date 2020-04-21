using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
