using System;
using System.Runtime.Serialization;

namespace Relativity.Sync.Executors
{
    /// <summary>
    ///     Exception thrown when calls to Kepler services fail
    /// </summary>
    [Serializable]
    public sealed class SyncKeplerException : Exception
    {
        /// <inheritdoc />
        public SyncKeplerException()
        {
        }

        /// <inheritdoc />
        public SyncKeplerException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public SyncKeplerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        private SyncKeplerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}