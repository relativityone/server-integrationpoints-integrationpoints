namespace Relativity.Sync.Storage
{
	internal class CreateJobHistoryErrorDto
	{
		public string ErrorMessage { get; set; }
		public ErrorType ErrorType { get; }
		public int JobHistoryArtifactId { get; }
		public string SourceUniqueId { get; set; }
		public string StackTrace { get; set; }

		public CreateJobHistoryErrorDto(int jobHistoryArtifactId, ErrorType errorType)
		{
			JobHistoryArtifactId = jobHistoryArtifactId;
			ErrorType = errorType;
		}
	}
}