using System;
using System.Runtime.Serialization;

namespace Relativity.Sync.Executors
{
	/// <summary>
	///     Exception thrown by methods of <see cref="IDestinationWorkspaceTagRepository" />
	///     when errors occur in external services.
	/// </summary>
	[Serializable]
	public sealed class DestinationWorkspaceTagRepositoryException : Exception
	{
		/// <inheritdoc />
		public DestinationWorkspaceTagRepositoryException()
		{
		}

		/// <inheritdoc />
		public DestinationWorkspaceTagRepositoryException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public DestinationWorkspaceTagRepositoryException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <inheritdoc />
		private DestinationWorkspaceTagRepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}