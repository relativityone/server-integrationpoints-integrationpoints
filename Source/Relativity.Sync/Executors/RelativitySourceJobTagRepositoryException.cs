using System;
using System.Runtime.Serialization;

namespace Relativity.Sync.Executors
{
    /// <summary>
    ///     Exception thrown by methods of <see cref="IRelativitySourceCaseTagRepository" />
    ///     when errors occur in external services.
    /// </summary>
    [Serializable]
    public sealed class RelativitySourceJobTagRepositoryException : Exception
    {
        /// <inheritdoc />
        public RelativitySourceJobTagRepositoryException()
        {
        }

        /// <inheritdoc />
        public RelativitySourceJobTagRepositoryException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public RelativitySourceJobTagRepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        private RelativitySourceJobTagRepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}