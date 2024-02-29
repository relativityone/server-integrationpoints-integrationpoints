using System;
using System.Runtime.Serialization;

namespace Relativity.Sync.Executors
{
    /// <summary>
    ///     Exception thrown by methods of <see cref="IDestinationWorkspaceTagsLinker" />
    ///     when errors occur in external services.
    /// </summary>
    [Serializable]
    public sealed class DestinationWorkspaceTagsLinkerException : Exception
    {
        /// <inheritdoc />
        public DestinationWorkspaceTagsLinkerException()
        {
        }

        /// <inheritdoc />
        public DestinationWorkspaceTagsLinkerException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public DestinationWorkspaceTagsLinkerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        private DestinationWorkspaceTagsLinkerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
