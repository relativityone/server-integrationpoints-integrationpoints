using System;
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
		public ValidationResult ValidationResult { get; } = new ValidationResult();

		/// <inheritdoc />
		public ValidationException()
		{
		}

		/// <inheritdoc />
		public ValidationException(string message) : base(message)
		{
		}

		/// <inheritdoc />
		public ValidationException(string message, Exception innerException) : base(message, innerException)
		{
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
			foreach (ValidationMessage validationMessage in ValidationResult.Messages)
			{
				messages.AppendLine($"Error code: {validationMessage.ErrorCode}{Environment.NewLine}Message: {validationMessage.ShortMessage}");
			}

			return $"Is valid: {ValidationResult.IsValid}{Environment.NewLine}{messages}";
		}
	}
}