using System;

namespace Relativity.Sync.Executors
{
	/// <summary>
	///     Exception thrown by methods of <see cref="IDestinationWorkspaceTagRepository"/>
	///     when errors occur in external services.
	/// </summary>
	[Serializable]
	public sealed class DestinationWorkspaceTagRepositoryException : Exception
	{
		/// <inheritdoc />
		public DestinationWorkspaceTagRepositoryException() : base()
		{
		}

		/// <inheritdoc />
		public DestinationWorkspaceTagRepositoryException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public DestinationWorkspaceTagRepositoryException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		/// <inheritdoc />
		private DestinationWorkspaceTagRepositoryException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
