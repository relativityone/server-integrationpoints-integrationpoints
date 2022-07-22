using System;

namespace Relativity.Sync.Transfer
{
    /// <summary>
    ///    Exception thrown when the <see cref="SourceWorkspaceDataReader"/> fails to create a child DataTable
    ///    from a batch of objects.
    /// </summary>
    [Serializable]
    public sealed class SourceDataReaderException : Exception
    {
        /// <inheritdoc />
        public SourceDataReaderException()
        {
        }

        /// <inheritdoc />
        public SourceDataReaderException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public SourceDataReaderException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        private SourceDataReaderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
