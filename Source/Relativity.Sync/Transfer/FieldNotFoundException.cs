using System;

namespace Relativity.Sync.Transfer
{
    /// <summary>
    /// Exception thrown by <see cref="IObjectFieldTypeRepository"/> when a given field is not found in a workspace.
    /// </summary>
    [Serializable]
    public sealed class FieldNotFoundException : Exception
    {
        /// <inheritdoc />
        public FieldNotFoundException()
        {
        }

        /// <inheritdoc />
        public FieldNotFoundException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public FieldNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        private FieldNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}
