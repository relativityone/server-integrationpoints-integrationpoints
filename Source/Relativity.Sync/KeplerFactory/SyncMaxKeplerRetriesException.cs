using System;
using System.Runtime.Serialization;

namespace Relativity.Sync.KeplerFactory
{
    /// <summary>
    /// Exception thrown when maximum number of Kepler Service retries has occured.
    /// </summary>
    [Serializable]
    public sealed class SyncMaxKeplerRetriesException : Exception
    {
        /// <inheritdoc />
        public SyncMaxKeplerRetriesException()
        {
        }

        /// <inheritdoc />
        public SyncMaxKeplerRetriesException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public SyncMaxKeplerRetriesException(string kepler, int numberOfRetries)
            : base($"Maximum number of retries ({numberOfRetries}) has been performed for {kepler} Kepler Service")
        {
        }

        /// <inheritdoc />
        public SyncMaxKeplerRetriesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc />
        public SyncMaxKeplerRetriesException(string kepler, int numberOfRetries, Exception innerException)
            : base($"Maximum number of retries ({numberOfRetries}) has been performed for {kepler} Kepler Service", innerException)
        {
        }

        private SyncMaxKeplerRetriesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
