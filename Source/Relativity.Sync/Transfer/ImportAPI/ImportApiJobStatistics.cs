using System;

namespace Relativity.Sync.Transfer.ImportAPI
{
	internal sealed class ImportApiJobStatistics
	{
		public int TotalItemsCount { set; get; }
		public int ErrorItemsCount { get; set; }
		public int CompletedItemsCount => TotalItemsCount - ErrorItemsCount;
		public long MetadataBytes { get; set; }
		public long FileBytes { get; set; }
		public Exception Exception { get; set; }

		internal static ImportApiJobStatistics FromJobReport(JobReport jobReport)
		{
			ImportApiJobStatistics statistics = new ImportApiJobStatistics()
			{
				TotalItemsCount = jobReport.TotalRows,
				ErrorItemsCount = jobReport.ErrorRowCount,
				FileBytes = jobReport.FileBytes,
				MetadataBytes = jobReport.MetadataBytes,
				Exception = jobReport.FatalException,
			};
			return statistics;
		}

	}
}