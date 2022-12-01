using System;

namespace Relativity.Sync.Transfer.ImportAPI
{
    internal class ImportApiJobStatistics
    {
        public ImportApiJobStatistics(int totalItemsCount, int errorItemsCount, long metadataBytes, long fileBytes, Exception exception = null)
        {
            TotalItemsCount = totalItemsCount;
            ErrorItemsCount = errorItemsCount;
            MetadataBytes = metadataBytes;
            FileBytes = fileBytes;
            Exception = exception;
        }

        public int TotalItemsCount { get; }

        public int ErrorItemsCount { get; }

        public int CompletedItemsCount => TotalItemsCount - ErrorItemsCount;

        public long MetadataBytes { get; }

        public long FileBytes { get; }

        public Exception Exception { get; }
    }
}
