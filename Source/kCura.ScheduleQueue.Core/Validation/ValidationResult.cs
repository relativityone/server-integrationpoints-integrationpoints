namespace kCura.ScheduleQueue.Core.Validation
{
	public class ValidationResult
	{
		public bool IsValid { get; set; }

		public string Message { get; set; }

        public bool CreateValidationFailedJobHistory { get;}

		private ValidationResult(bool isValid, string message)
		{
			IsValid = isValid;
			Message = message;
		}

        private ValidationResult(bool isValid, string message, bool createValidationFailedJobHistory)
        {
            IsValid = isValid;
            Message = message;
            CreateValidationFailedJobHistory = createValidationFailedJobHistory;
        }

		public static ValidationResult Success => new ValidationResult(true, string.Empty);

		public static ValidationResult Failed(string message, bool createValidationFailedJobHistory = false) => new ValidationResult(false, message, createValidationFailedJobHistory);
    }
}
