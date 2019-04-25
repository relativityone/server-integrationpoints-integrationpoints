using System.Threading.Tasks;
using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal sealed class BatchProgressHandler : IBatchProgressHandler
	{
		private readonly IBatch _batch;

		public BatchProgressHandler(IBatch batch, IImportNotifier importNotifier)
		{
			_batch = batch;
			importNotifier.OnProcessProgress += HandleProcessProgress;
		}

		private void HandleProcessProgress(FullStatus status)
		{
			int totalRecordsProcessed = (int)status.TotalRecordsProcessed; // completed + failed records
			int recordsWithErrors = (int)status.TotalRecordsProcessedWithErrors;
			int completedRecords = totalRecordsProcessed - recordsWithErrors;

			Task setCompletedItems = _batch.SetTransferredItemsCountAsync(completedRecords);
			Task setFailedItems = _batch.SetFailedItemsCountAsync(recordsWithErrors);
			double progress = CalculateProgress(totalRecordsProcessed, _batch.TotalItemsCount);
			Task setProgress = _batch.SetProgressAsync(progress);

			Task.WaitAll(setCompletedItems, setFailedItems, setProgress);
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