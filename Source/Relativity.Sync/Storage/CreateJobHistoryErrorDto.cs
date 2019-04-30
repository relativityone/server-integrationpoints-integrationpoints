namespace Relativity.Sync.Storage
{
	internal class CreateJobHistoryErrorDto
	{
		public string ErrorMessage { get; }
		public ErrorType ErrorType { get; }
		public int JobHistoryArtifactId { get; }
		public string SourceUniqueId { get; }
		public string StackTrace { get; }
	}
}