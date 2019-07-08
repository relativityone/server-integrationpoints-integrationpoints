﻿using System;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal sealed class BatchProgressUpdater : IBatchProgressUpdater
	{
		private readonly ISyncLog _logger;
		private readonly ISemaphoreSlim _semaphoreSlim;

		public BatchProgressUpdater(ISyncLog logger, ISemaphoreSlim semaphoreSlim)
		{
			_logger = logger;
			_semaphoreSlim = semaphoreSlim;
		}

		public async Task UpdateProgressAsync(IBatch batch, int completedRecordsCount, int failedRecordsCount)
		{
			// Currently IAPI reports wrong number of records processed, records with errors, and total number of records.
			// Related Jira item: REL-286003

			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

			try
			{
				await batch.SetTransferredItemsCountAsync(completedRecordsCount).ConfigureAwait(false);
				await batch.SetFailedItemsCountAsync(failedRecordsCount).ConfigureAwait(false);
				double progress = CalculateProgress(completedRecordsCount + failedRecordsCount, batch.TotalItemsCount);
				await batch.SetProgressAsync(progress).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception occurred while updating import job progress.");
			}

			_semaphoreSlim.Release();
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