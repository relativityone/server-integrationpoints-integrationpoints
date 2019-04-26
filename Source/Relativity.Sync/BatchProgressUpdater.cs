using System;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal sealed class BatchProgressUpdater : IBatchProgressUpdater
	{
		private readonly IBatch _batch;
		private readonly ISyncLog _logger;

		public BatchProgressUpdater(IBatch batch, ISyncLog logger)
		{
			_batch = batch;
			_logger = logger;
		}

		public async Task UpdateProgressAsync(int completedRecordsCount, int failedRecordsCount)
		{
			// Currently IAPI reports wrong number of records processed, records with errors, and total number of records. 
			// Related Jira item: REL-286003

			try
			{
				await _batch.SetTransferredItemsCountAsync(completedRecordsCount).ConfigureAwait(false);
				await _batch.SetFailedItemsCountAsync(failedRecordsCount).ConfigureAwait(false);
				double progress = CalculateProgress(completedRecordsCount + failedRecordsCount, _batch.TotalItemsCount);
				await _batch.SetProgressAsync(progress).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception occurred while updating import job progress.");
			}
		}

		private static double CalculateProgress(int processedRecordsCount, int totalRecordsCount)
		{
			if (totalRecordsCount == 0)
			{
				return 0;
			}

			const int percentage = 100;
			double progress = (double)processedRecordsCount / totalRecordsCount * percentage;
			return progress;
		}
	}
}