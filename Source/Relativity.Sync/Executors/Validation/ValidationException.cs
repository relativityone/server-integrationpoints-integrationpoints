using System;
using System.Runtime.Serialization;

namespace Relativity.Sync.Executors.Validation
{
	/// <summary>
	/// Exception thrown by <see cref="ValidationExecutor"/> when validation fails.
	/// </summary>
	[Serializable]
	public sealed class ValidationException : Exception
	{
		/// <inheritdoc />
		public ValidationException()
		{
		}

		/// <inheritdoc />
		public ValidationException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public ValidationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <inheritdoc />
		private ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}