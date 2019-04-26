using System;
using System.Threading.Tasks;
using kCura.Relativity.DataReaderClient;
using Relativity.Services.Exceptions;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal sealed class BatchProgressHandler : IBatchProgressHandler
	{
		private readonly IBatch _batch;
		private readonly ISyncLog _logger;

		public BatchProgressHandler(IBatch batch, IImportNotifier importNotifier, ISyncLog logger)
		{
			_batch = batch;
			_logger = logger;
			importNotifier.OnProcessProgress += HandleProcessProgress;
		}

		private void HandleProcessProgress(FullStatus status)
		{
			// Currently IAPI reports wrong number of records processed, records with errors, and total number of records. 
			// Related Jira item: REL-286003

			try
			{
				int totalRecordsProcessed = (int)status.TotalRecordsProcessed; // completed + failed records
				int recordsWithErrors = (int)status.TotalRecordsProcessedWithErrors;
				int completedRecords = totalRecordsProcessed - recordsWithErrors;

				_batch.SetTransferredItemsCountAsync(completedRecords).ConfigureAwait(false).GetAwaiter().GetResult();
				_batch.SetFailedItemsCountAsync(recordsWithErrors).ConfigureAwait(false).GetAwaiter().GetResult();
				double progress = CalculateProgress(totalRecordsProcessed, _batch.TotalItemsCount);
				_batch.SetProgressAsync(progress).ConfigureAwait(false).GetAwaiter().GetResult();
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