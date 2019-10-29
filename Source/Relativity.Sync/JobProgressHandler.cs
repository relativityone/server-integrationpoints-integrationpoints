using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Executors;

namespace Relativity.Sync
{
	internal sealed class JobProgressHandler : IJobProgressHandler
	{
		private readonly int _throttleForSeconds = 5;
		private readonly IJobProgressUpdater _jobProgressUpdater;
		private readonly Subject<Unit> _changeSignal = new Subject<Unit>();
		private readonly Subject<Unit> _forceUpdateSignal = new Subject<Unit>();
		private readonly IDisposable _changeSubjectBufferSubscription;
		private readonly IDictionary<int, SyncBatchProgress> _batchProgresses = new ConcurrentDictionary<int, SyncBatchProgress>();

		public JobProgressHandler(IJobProgressUpdater jobProgressUpdater, IScheduler timerScheduler = null)
		{
			_jobProgressUpdater = jobProgressUpdater;

			_changeSubjectBufferSubscription = _changeSignal.Buffer(TimeSpan.FromSeconds(_throttleForSeconds), timerScheduler ?? Scheduler.Default)
				.Where(changesInTimeWindow => changesInTimeWindow.Any())
				.Select(_ => Unit.Default)
				.Merge(_forceUpdateSignal)
				.Do(async (_) =>
			{
				await UpdateProgress().ConfigureAwait(false);
			})
				.Subscribe();
		}



		public IDisposable AttachToImportJob(ISyncImportBulkArtifactJob job, int batchId, int totalItemsInBatch)
		{
			SyncBatchProgress batchProgress = new SyncBatchProgress(batchId, totalItemsInBatch);

			IDisposable itemProcessedSubscription = Observable
				.FromEvent<IImportNotifier.OnProgressEventHandler, long>(
					handler => job.OnProgress += handler,
					 handler => job.OnProgress -= handler)
					.Do(_ =>
				{
					batchProgress.ItemsProcessed++;
					_changeSignal.OnNext(Unit.Default);
				}).Subscribe();

			IDisposable itemFailedSubscription = Observable
				.FromEvent<ImportBulkArtifactJob.OnErrorEventHandler, IDictionary>(
					 handler => job.OnError += handler,
					 handler => job.OnError -= handler)
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

			IObservable<JobReport> batchCompletedReports = Observable
				.FromEvent<IImportNotifier.OnCompleteEventHandler, JobReport>(
					handler => job.OnComplete += handler,
					 handler => job.OnComplete -= handler)
					.Do(_ =>
				{
					batchProgress.Completed = true;
				}
					);

			IDisposable jobReportsSubscription = Observable
				.FromEvent<IImportNotifier.OnFatalExceptionEventHandler, JobReport>(
					 handler => job.OnFatalException += handler,
					 handler => job.OnFatalException -= handler)
					.Merge(batchCompletedReports)
					.Do(jobReport =>
				{
					batchProgress.ItemsProcessed = jobReport.TotalRows - jobReport.ErrorRowCount;
					batchProgress.ItemsFailed = jobReport.ErrorRowCount;
					_forceUpdateSignal.OnNext(Unit.Default);
				})
					.Subscribe();

			_batchProgresses[batchId] = batchProgress;

			_forceUpdateSignal.OnNext(Unit.Default);

			return new CompositeDisposable(itemFailedSubscription, itemProcessedSubscription, jobReportsSubscription);
		}

		private async Task UpdateProgress()
		{
			int totalProcessedItems = _batchProgresses.Values.Sum(x => x.ItemsProcessed);
			int totalFailedItems = _batchProgresses.Values.Sum(x => x.ItemsFailed);

			await _jobProgressUpdater.UpdateJobProgressAsync(totalProcessedItems, totalFailedItems).ConfigureAwait(false);
		}

		public void Dispose()
		{
			_changeSubjectBufferSubscription.Dispose();
			_changeSignal.Dispose();
			_forceUpdateSignal.Dispose();
		}
	}
}