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
			int totalProcessedRecords = (int)status.TotalRecordsProcessed;
			const int percentage = 100;
			double progress = (double)totalProcessedRecords / _batch.TotalItemsCount * percentage;

			Task setCompletedItems = _batch.SetTransferredItemsCountAsync(totalProcessedRecords);
			Task setFailedItems = _batch.SetFailedItemsCountAsync((int)status.TotalRecordsProcessedWithErrors);
			Task setProgress = _batch.SetProgressAsync(progress);

			Task.WaitAll(setCompletedItems, setFailedItems, setProgress);
		}
	}
}