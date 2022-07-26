using System;
using System.Runtime.Serialization;

namespace Relativity.Sync.Executors
{
    /// <summary>
    ///     Exception thrown by methods of <see cref="IRelativitySourceCaseTagRepository" />
    ///     when errors occur in external services.
    /// </summary>
    [Serializable]
    public sealed class RelativitySourceCaseTagRepositoryException : Exception
    {
        /// <inheritdoc />
        public RelativitySourceCaseTagRepositoryException()
        {
        }

        /// <inheritdoc />
        public RelativitySourceCaseTagRepositoryException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public RelativitySourceCaseTagRepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        private RelativitySourceCaseTagRepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}