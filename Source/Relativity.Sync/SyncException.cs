using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Relativity.Sync
{
	/// <summary>
	///     Represents Sync exception
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable]
	public sealed class SyncException : Exception
	{
		/// <summary>
		///     Constructor
		/// </summary>
		public SyncException()
		{
		}

		/// <summary>
		///     Constructor
		/// </summary>
		private SyncException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		/// <summary>
		///     Constructor
		/// </summary>
		public SyncException(string message) : base(message)
		{
		}

		/// <summary>
		///     Constructor
		/// </summary>
		public SyncException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}