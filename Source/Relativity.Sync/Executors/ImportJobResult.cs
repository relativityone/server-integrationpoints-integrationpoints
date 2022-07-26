namespace Relativity.Sync.Executors
{
    internal sealed class ImportJobResult
    {
        public ExecutionResult ExecutionResult { get; }

        public long MetadataSizeInBytes { get; }

        public long FilesSizeInBytes { get; }

        public long JobSizeInBytes { get; }

        public ImportJobResult(ExecutionResult executionResult, long metadataSizeInBytes, long filesSizeInBytes, long jobSizeInBytes)
        {
            ExecutionResult = executionResult;
            MetadataSizeInBytes = metadataSizeInBytes;
            FilesSizeInBytes = filesSizeInBytes;
            JobSizeInBytes = jobSizeInBytes;
        }
    }
}