using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Relativity.Sync.Tests.System
{
	[Serializable]
	public sealed class SystemTestException : Exception
	{
		public SystemTestException()
		{
		}

		public SystemTestException(string message) : base(message)
		{
		}

		public SystemTestException(string message, Exception inner) : base(message, inner)
		{
		}

		private SystemTestException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}