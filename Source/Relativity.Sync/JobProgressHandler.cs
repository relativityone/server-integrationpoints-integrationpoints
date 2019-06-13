using System;
using System.Collections;

namespace Relativity.Sync
{
	// TODO REL-292382 This is a temporary solution for updating job history, until we implement REL-292382.
	internal sealed class JobProgressHandler : IJobProgressHandler
	{
		private DateTime _lastProgressEventTimestamp = DateTime.MinValue;
		private int _itemsProcessedCount = 0;
		private int _itemsFailedCount = 0;

		private readonly IJobProgressUpdater _jobProgressUpdater;
		private readonly IDateTime _dateTime;
		private readonly TimeSpan _throttle = TimeSpan.FromSeconds(1);

		public JobProgressHandler(IJobProgressUpdater jobProgressUpdater, IDateTime dateTime)
		{
			_jobProgressUpdater = jobProgressUpdater;
			_dateTime = dateTime;
		}

		// Explanation of how IAPI works:
		// IAPI processes items in batch one by one and fires OnProgress event after each item that was
		// successfully read from data reader. OnProgress is not fired when IAPI fails to read record from
		// data reader. This is handled in HandleItemProcessed method below, where we are incrementing number of processed items.
		// When IAPI completes processing of batch, it checks each item if it was successfully processed. If not, it fires OnError event for each of the items.
		// That's why in HandleItemError method, we are decrementing number of successfully processed items and
		// incrementing number of failed items.

		public void HandleItemProcessed(long item)
		{
			_itemsProcessedCount++;
			UpdateProgressIfPossible();
		}

		public void HandleItemError(IDictionary row)
		{
			_itemsFailedCount++;
			if (_itemsProcessedCount > 0)
			{
				_itemsProcessedCount--;
			}
			UpdateProgressIfPossible();
		}

		public void HandleProcessComplete(JobReport jobReport)
		{
			UpdateProgress();
		}

		private void UpdateProgressIfPossible()
		{
			bool canUpdate = CanUpdateProgress();
			if (canUpdate)
			{
				UpdateProgress();
			}
		}

		private bool CanUpdateProgress()
		{
			return _dateTime.Now >= _lastProgressEventTimestamp + _throttle;
		}

		private void UpdateProgress()
		{
			_jobProgressUpdater.UpdateJobProgressAsync(_itemsProcessedCount, _itemsFailedCount).ConfigureAwait(false).GetAwaiter().GetResult();
			_lastProgressEventTimestamp = _dateTime.Now;
		}
	}
}