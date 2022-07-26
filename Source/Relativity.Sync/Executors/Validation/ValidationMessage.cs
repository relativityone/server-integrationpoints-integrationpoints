using System;

namespace Relativity.Sync.Executors.Validation
{
    /// <summary>
    /// Contains message from <see cref="IValidator"/>
    /// </summary>
    [Serializable]
    public sealed class ValidationMessage : IEquatable<ValidationMessage>
    {
        /// <summary>
        /// Default constructor used for JSON serialization.
        /// </summary>
        public ValidationMessage()
        {
        }

        /// <summary>
        /// Creates instance of this class with short message.
        /// </summary>
        /// <param name="shortMessage">Short message.</param>
        public ValidationMessage(string shortMessage) : this(string.Empty, shortMessage)
        {
        }

        /// <summary>
        /// Creates instance of this class with error code and short message.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        /// <param name="shortMessage">Short message.</param>
        public ValidationMessage(string errorCode, string shortMessage)
        {
            ErrorCode = errorCode;
            ShortMessage = shortMessage;
        }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the short message.
        /// </summary>
        public string ShortMessage { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.IsNullOrEmpty(ErrorCode) ? ShortMessage : $"{ErrorCode} {ShortMessage}";
        }

        /// <inheritdoc />
        public bool Equals(ValidationMessage other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(ErrorCode, other.ErrorCode, StringComparison.InvariantCulture) && string.Equals(ShortMessage, other.ShortMessage, StringComparison.InvariantCulture);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((ValidationMessage)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            const int multiplier = 397;
            unchecked
            {
                return ((ErrorCode != null ? ErrorCode.GetHashCode() : 0) * multiplier) ^ (ShortMessage != null ? ShortMessage.GetHashCode() : 0);
            }
        }
    }
}