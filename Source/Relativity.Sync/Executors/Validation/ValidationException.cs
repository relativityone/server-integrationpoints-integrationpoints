using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Relativity.Sync.Executors.Validation
{
    /// <summary>
    /// Exception thrown by <see cref="ValidationExecutor"/> when validation fails.
    /// </summary>
    [Serializable]
    public sealed class ValidationException : Exception
    {
        /// <summary>
        /// Holds information about validation.
        /// </summary>
        public ValidationResult ValidationResult { get; }

        /// <inheritdoc />
        public ValidationException()
        {
            ValidationResult = ValidationResult.Invalid;
        }

        /// <inheritdoc />
        public ValidationException(string message) : base(message)
        {
            ValidationResult = ValidationResult.Invalid;
        }

        /// <inheritdoc />
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            ValidationResult = ValidationResult.Invalid;
        }

        /// <inheritdoc />
        public ValidationException(ValidationResult validationResult)
        {
            ValidationResult = validationResult;
        }

        /// <inheritdoc />
        public ValidationException(string message, ValidationResult validationResult) : base(message)
        {
            ValidationResult = validationResult;
        }

        /// <inheritdoc />
        public ValidationException(string message, Exception innerException, ValidationResult validationResult) : base(message, innerException)
        {
            ValidationResult = validationResult;
        }

        /// <inheritdoc />
        private ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ValidationResult = (ValidationResult) info.GetValue(nameof(ValidationResult), typeof(ValidationResult));
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(ValidationResult), ValidationResult);
            base.GetObjectData(info, context);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder messages = new StringBuilder();
            List<ValidationMessage> validationMessages = ValidationResult.Messages.ToList();

            for (int i = 0; i < validationMessages.Count; i++)
            {
                ValidationMessage message = validationMessages[i];
                string errorCode = string.IsNullOrEmpty(message.ErrorCode) ? string.Empty : $"(Error code: {message.ErrorCode}) ";

                messages.AppendLine($"{i+1}. {errorCode}{message.ShortMessage}");
            }

            return $"Is valid: {ValidationResult.IsValid}{System.Environment.NewLine}{messages}";
        }
    }
}