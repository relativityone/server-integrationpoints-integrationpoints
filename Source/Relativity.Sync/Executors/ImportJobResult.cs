namespace Relativity.Sync.Executors
{
	internal sealed class ImportJobResult
	{
		public ImportJobResult(ExecutionResult executionResult, long jobSizeInBytes)
		{
			ExecutionResult = executionResult;
			JobSizeInBytes = jobSizeInBytes;
		}

		public ExecutionResult ExecutionResult { get; }
		public long JobSizeInBytes { get; }
	}
}