using System;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class ValidationMessage
	{
		public ValidationMessage(string shortMessage) : this(string.Empty, shortMessage)
		{
		}

		public ValidationMessage(string errorCode, string shortMessage)
		{
			ErrorCode = errorCode;
			ShortMessage = shortMessage;
		}

		public string ErrorCode { get; set; }

		public string ShortMessage { get; set; }

		public override string ToString()
		{
			return string.IsNullOrEmpty(ErrorCode) ? ShortMessage : $"{ErrorCode} {ShortMessage}";
		}

		private bool Equals(ValidationMessage other)
		{
			return string.Equals(ErrorCode, other.ErrorCode, StringComparison.InvariantCulture) && string.Equals(ShortMessage, other.ShortMessage, StringComparison.InvariantCulture);
		}

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