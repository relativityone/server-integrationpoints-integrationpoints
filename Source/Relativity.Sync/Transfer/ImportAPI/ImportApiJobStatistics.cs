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

		public int TotalItemsCount { set; get; }
		public int ErrorItemsCount { get; set; }
		public int CompletedItemsCount => TotalItemsCount - ErrorItemsCount;
		public long MetadataBytes { get; set; }
		public long FileBytes { get; set; }
		public Exception Exception { get; set; }
	}
}