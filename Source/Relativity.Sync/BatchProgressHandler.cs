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
			int totalProcessedRecords = (int)status.TotalRecordsProcessed; // completed + failed records
			int recordsWithErrors = (int)status.TotalRecordsProcessedWithErrors;
			int completedRecords = _batch.TotalItemsCount - recordsWithErrors;
			const int percentage = 100;
			double progress = (double)totalProcessedRecords / _batch.TotalItemsCount * percentage;

			Task setCompletedItems = _batch.SetTransferredItemsCountAsync(completedRecords);
			Task setFailedItems = _batch.SetFailedItemsCountAsync(recordsWithErrors);
			Task setProgress = _batch.SetProgressAsync(progress);

			Task.WaitAll(setCompletedItems, setFailedItems, setProgress);
		}
	}
}