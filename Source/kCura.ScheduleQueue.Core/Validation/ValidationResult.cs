namespace kCura.ScheduleQueue.Core.Validation
{
	public class ValidationResult
	{
		public bool IsValid { get; set; }

		public string Message { get; set; }

		private ValidationResult(bool isValid, string message)
		{
			IsValid = isValid;
			Message = message;
		}

		public static ValidationResult Success => new ValidationResult(true, string.Empty);

		public static ValidationResult Failed(string message) => new ValidationResult(false, message);
	}
}
