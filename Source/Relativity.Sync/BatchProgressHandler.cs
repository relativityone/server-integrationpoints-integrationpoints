using System;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync
{
	internal sealed class BatchProgressHandler : IBatchProgressHandler
	{
		private DateTime _lastProgressEventTimestamp;
		private int _completedRecordsCount;
		private int _failedRecordsCount;
		
		private readonly IBatchProgressUpdater _progressUpdater;
		private readonly IDateTime _dateTime;
		private readonly TimeSpan _throttle = TimeSpan.FromSeconds(1);

		public BatchProgressHandler(IImportNotifier importNotifier, IBatchProgressUpdater progressUpdater, IDateTime dateTime)
		{
			_progressUpdater = progressUpdater;
			_dateTime = dateTime;
			importNotifier.OnProcessProgress += HandleProcessProgress;
			importNotifier.OnComplete += HandleProcessComplete;
		}

		private void HandleProcessProgress(FullStatus status)
		{
			_completedRecordsCount = (int)status.TotalRecordsProcessed - (int)status.TotalRecordsProcessedWithErrors;
			_failedRecordsCount = (int)status.TotalRecordsProcessedWithErrors;

			if (CanUpdateProgress())
			{
				UpdateProgress();
			}
		}

		private bool CanUpdateProgress()
		{
			return _dateTime.Now >= _lastProgressEventTimestamp + _throttle;
		}

		private void HandleProcessComplete(JobReport jobreport)
		{
			_failedRecordsCount = jobreport.ErrorRowCount;
			_completedRecordsCount = jobreport.TotalRows - jobreport.ErrorRowCount;
			UpdateProgress();
		}

		private void UpdateProgress()
		{
			_progressUpdater.UpdateProgressAsync(_completedRecordsCount, _failedRecordsCount).ConfigureAwait(false).GetAwaiter().GetResult();
			_lastProgressEventTimestamp = _dateTime.Now;
		}
	}
}