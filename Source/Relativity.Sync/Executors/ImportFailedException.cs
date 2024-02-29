using System;
using System.Runtime.Serialization;

namespace Relativity.Sync.Executors
{
    /// <inheritdoc />
    [Serializable]
    public sealed class ImportFailedException : Exception
    {
        /// <inheritdoc />
        public ImportFailedException()
        {
        }

        /// <inheritdoc />
        public ImportFailedException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public ImportFailedException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <inheritdoc />
        private ImportFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
