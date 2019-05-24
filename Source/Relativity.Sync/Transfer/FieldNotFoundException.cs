using System;

namespace Relativity.Sync.Transfer
{
	/// <inheritdoc />
	[Serializable]
	public sealed class FieldNotFoundException : Exception
	{
		/// <inheritdoc />
		public FieldNotFoundException() : base()
		{
			//...
		}

		/// <inheritdoc />
		public FieldNotFoundException(string message) : base(message)
		{
			//...
		}

		/// <inheritdoc />
		public FieldNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
			//...
		}

		/// <inheritdoc />
		private FieldNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			//...
		}
	}
}
