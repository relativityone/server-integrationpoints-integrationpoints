namespace Relativity.Sync.Storage
{
    internal class CreateJobHistoryErrorDto
    {
        public string ErrorMessage { get; set; }

        public ErrorType ErrorType { get; }

        public string SourceUniqueId { get; set; }

        public string StackTrace { get; set; }

        public CreateJobHistoryErrorDto(ErrorType errorType)
        {
            ErrorType = errorType;
        }
    }
}
