using System;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync
{
	internal sealed class BatchProgressHandler : IBatchProgressHandler
	{
		private DateTime _lastProgressEventTimestamp = DateTime.Now;
		private int _completedRecordsCount;
		private int _failedRecordsCount;

		private readonly IBatchProgressUpdater _progressUpdater;

		internal TimeSpan Throttle { get; set; } = TimeSpan.FromSeconds(1);

		public BatchProgressHandler(IImportNotifier importNotifier, IBatchProgressUpdater progressUpdater)
		{
			_progressUpdater = progressUpdater;
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
				_lastProgressEventTimestamp = DateTime.Now;
			}
		}

		private bool CanUpdateProgress()
		{
			return DateTime.Now > _lastProgressEventTimestamp + Throttle;
		}

		private void HandleProcessComplete(JobReport jobreport)
		{
			UpdateProgress();
		}

		private void UpdateProgress()
		{
			_progressUpdater.UpdateProgressAsync(_completedRecordsCount, _failedRecordsCount).ConfigureAwait(false).GetAwaiter().GetResult();
		}
	}
}