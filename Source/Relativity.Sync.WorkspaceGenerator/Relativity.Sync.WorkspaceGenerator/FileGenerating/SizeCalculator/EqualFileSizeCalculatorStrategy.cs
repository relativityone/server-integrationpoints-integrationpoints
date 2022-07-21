namespace Relativity.Sync.WorkspaceGenerator.FileGenerating.SizeCalculator
{
    public class EqualFileSizeCalculatorStrategy : IFileSizeCalculatorStrategy
    {
        private readonly long _singleFileSizeInBytes;

        public EqualFileSizeCalculatorStrategy(int desiredFilesCount, long totalSizeInMB)
        {
            long totalSizeInBytes = totalSizeInMB * 1024 * 1024;
            _singleFileSizeInBytes = totalSizeInBytes / desiredFilesCount;
        }

        public long GetNext()
        {
            return _singleFileSizeInBytes;
        }
    }
}