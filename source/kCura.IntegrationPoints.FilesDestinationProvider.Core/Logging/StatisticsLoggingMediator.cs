using System;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	class StatisticsLoggingMediator : ILoggingMediator, IBatchReporter
	{
		public void RegisterEventHandlers(IUserMessageNotification userMessageNotification,
			ICoreExporterStatusNotification exporterStatusNotification)
		{
			exporterStatusNotification.OnBatchCompleted += OnOnBatchComplete;
		}

		private void OnOnBatchComplete(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount)
		{
			OnBatchComplete?.Invoke(startTime, endTime, totalRows, errorRowCount);
		}


		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit { add { } remove { } }
		public event BatchCreated OnBatchCreate { add { } remove { } }
		public event StatusUpdate OnStatusUpdate { add { } remove { } }
		public event JobError OnJobError { add { } remove { } }
		public event RowError OnDocumentError { add { } remove { } }
	}
}
