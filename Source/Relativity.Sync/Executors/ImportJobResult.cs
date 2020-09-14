namespace Relativity.Sync.Executors
{
	internal sealed class ImportJobResult
	{
		public ExecutionResult ExecutionResult { get; }

		public long MetadataSizeInBytes { get; }

		public long JobSizeInBytes { get; }

		public ImportJobResult(ExecutionResult executionResult, long metadataSizeInBytes, long jobSizeInBytes)
		{
			ExecutionResult = executionResult;
			MetadataSizeInBytes = metadataSizeInBytes;
			JobSizeInBytes = jobSizeInBytes;
		}
	}
}