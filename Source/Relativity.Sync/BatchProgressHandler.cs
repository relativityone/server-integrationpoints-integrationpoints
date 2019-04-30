using System;
using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal sealed class BatchProgressHandler : IBatchProgressHandler
	{
		private DateTime _lastProgressEventTimestamp = DateTime.MinValue;

		private readonly IBatch _batch;
		private readonly IBatchProgressUpdater _progressUpdater;
		private readonly IDateTime _dateTime;
		private readonly TimeSpan _throttle = TimeSpan.FromSeconds(1);

		public BatchProgressHandler(IBatch batch, IBatchProgressUpdater progressUpdater, IDateTime dateTime)
		{
			_batch = batch;
			_progressUpdater = progressUpdater;
			_dateTime = dateTime;
		}

		public void HandleProcessProgress(FullStatus status)
		{
			bool canUpdate = CanUpdateProgress();
			if (canUpdate)
			{
				int completedRecordsCount = (int)status.TotalRecordsProcessed - (int)status.TotalRecordsProcessedWithErrors;
				int failedRecordsCount = (int)status.TotalRecordsProcessedWithErrors;
				UpdateProgress(completedRecordsCount, failedRecordsCount);
			}
		}

		public void HandleProcessComplete(JobReport jobReport)
		{
			int failedRecordsCount = jobReport.ErrorRowCount;
			int completedRecordsCount = jobReport.TotalRows - jobReport.ErrorRowCount;
			UpdateProgress(completedRecordsCount, failedRecordsCount);
		}

		private bool CanUpdateProgress()
		{
			return _dateTime.Now >= _lastProgressEventTimestamp + _throttle;
		}

		private void UpdateProgress(int completedRecordsCount, int failedRecordsCount)
		{
			_progressUpdater.UpdateProgressAsync(_batch, completedRecordsCount, failedRecordsCount).ConfigureAwait(false).GetAwaiter().GetResult();
			_lastProgressEventTimestamp = _dateTime.Now;
		}
	}
}