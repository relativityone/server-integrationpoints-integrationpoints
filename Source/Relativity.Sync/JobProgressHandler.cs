using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync
{
	internal sealed class JobProgressHandler : IJobProgressHandler
	{
		private const int _THROTTHLE_FOR_SECONDS = 5;
		private readonly IJobProgressUpdater _jobProgressUpdater;
		private readonly Subject<Unit> _changeSignal = new Subject<Unit>();
		private readonly Subject<Unit> _forceUpdateSignal = new Subject<Unit>();
		private readonly IDisposable _changeSubjectBufferSubscription;
		private readonly IDictionary<int, SyncBatchProgress> _batchProgresses = new ConcurrentDictionary<int, SyncBatchProgress>();

		public JobProgressHandler(IJobProgressUpdater jobProgressUpdater, IEnumerable<IBatch> alreadyExecutedBatches, IScheduler timerScheduler = null)
		{
			_jobProgressUpdater = jobProgressUpdater;

			foreach (IBatch alreadyExecutedBatch in alreadyExecutedBatches)
			{
				_batchProgresses[alreadyExecutedBatch.ArtifactId] = new SyncBatchProgress(
						alreadyExecutedBatch.ArtifactId, totalItems:
						alreadyExecutedBatch.TotalDocumentsCount,
						alreadyExecutedBatch.FailedItemsCount,
						alreadyExecutedBatch.TransferredItemsCount
					)
					{Completed = true};
			}

			_changeSubjectBufferSubscription = _changeSignal.Buffer(TimeSpan.FromSeconds(_THROTTHLE_FOR_SECONDS), timerScheduler ?? Scheduler.Default)
				.Where(changesInTimeWindow => changesInTimeWindow.Any())
				.Select(_ => Unit.Default)
				.Merge(_forceUpdateSignal)
				.Do(async (_) => { await UpdateProgressAsync().ConfigureAwait(false); })
				.Subscribe();
		}

		public int GetBatchItemsProcessedCount(int batchId)
		{
			return _batchProgresses.ContainsKey(batchId) ? _batchProgresses[batchId].ItemsProcessed : 0;
		}

		public int GetBatchItemsFailedCount(int batchId)
		{
			return _batchProgresses.ContainsKey(batchId) ? _batchProgresses[batchId].ItemsFailed : 0;
		}

		public IDisposable AttachToImportJob(ISyncImportBulkArtifactJob job, IBatch batch)
		{
			SyncBatchProgress batchProgress = new SyncBatchProgress(batch.ArtifactId, totalItems: batch.TotalDocumentsCount,
				failedItemsCount: batch.FailedItemsCount, transferredItemsCount: batch.TransferredItemsCount);

			IDisposable itemProcessedSubscription = Observable
				.FromEvent<SyncJobEventHandler<ImportApiJobProgress>, ImportApiJobProgress>(
					handler => job.OnProgress += handler,
					handler => job.OnProgress -= handler)
				.Do(_ =>
				{
					batchProgress.ItemsProcessed++;
					_changeSignal.OnNext(Unit.Default);
				}).Subscribe();

			IDisposable itemFailedSubscription = Observable
				.FromEvent<SyncJobEventHandler<ItemLevelError>, ItemLevelError>(
					handler => job.OnItemLevelError += handler,
					handler => job.OnItemLevelError -= handler)
				.Do(_ =>
				{
					// Explanation of how IAPI works:
					// IAPI processes items in batch one by one and fires OnProgress event after each item that was
					// successfully read from data reader. OnProgress is not fired when IAPI fails to read record from
					// data reader. This is handled in subscription above.
					// When IAPI completes processing of batch, it checks each item if it was successfully processed. If not, it fires OnError event for each of the items.
					// That's why in HandleItemError method, we are decrementing number of successfully processed items and
					// incrementing number of failed items.

					batchProgress.ItemsFailed++;
					batchProgress.ItemsProcessed--;
					_changeSignal.OnNext(Unit.Default);
				}).Subscribe();

			IObservable<ImportApiJobStatistics> batchCompletedReports = Observable
				.FromEvent<SyncJobEventHandler<ImportApiJobStatistics>, ImportApiJobStatistics>(
					handler => job.OnComplete += handler,
					handler => job.OnComplete -= handler)
				.Do(_ =>
					{
						batchProgress.Completed = true;
					}
				);

			IDisposable jobReportsSubscription = Observable
				.FromEvent<SyncJobEventHandler<ImportApiJobStatistics>, ImportApiJobStatistics>(
					handler => job.OnFatalException += handler,
					handler => job.OnFatalException -= handler)
				.Merge(batchCompletedReports)
				.Do(statistics =>
				{
					batchProgress.ItemsProcessed = statistics.CompletedItemsCount;
					batchProgress.ItemsFailed = statistics.ErrorItemsCount;
					_forceUpdateSignal.OnNext(Unit.Default);
				})
				.Subscribe();

			_batchProgresses[batch.ArtifactId] = batchProgress;

			_forceUpdateSignal.OnNext(Unit.Default);

			return new CompositeDisposable(itemFailedSubscription, itemProcessedSubscription, jobReportsSubscription);
		}

		private Task UpdateProgressAsync()
		{
			int totalProcessedItems = _batchProgresses.Values.Sum(x => x.ItemsAlreadyProcessed + x.ItemsProcessed);
			int totalFailedItems = _batchProgresses.Values.Sum(x => x.ItemsAlreadyFailed + x.ItemsFailed);

			return _jobProgressUpdater.UpdateJobProgressAsync(totalProcessedItems, totalFailedItems);
		}

		public void Dispose()
		{
			_changeSubjectBufferSubscription?.Dispose();
			_changeSignal?.Dispose();
			_forceUpdateSignal?.Dispose();
		}
	}
}