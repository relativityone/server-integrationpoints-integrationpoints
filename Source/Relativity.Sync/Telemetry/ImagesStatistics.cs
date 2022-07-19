namespace Relativity.Sync.Telemetry
{
    internal readonly struct ImagesStatistics
    {
        public long TotalCount { get; }

        public long TotalSize { get; }

        public ImagesStatistics(long totalCount, long totalSize)
        {
            TotalCount = totalCount;
            TotalSize = totalSize;
        }
    }
}
