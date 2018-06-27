using System;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.Windows.Process;
using kCura.WinEDDS;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	public class StatisticsLoggingMediator : ILoggingMediator, IBatchReporter
	{
		#region Fields

		private int _currentExportedItemChunkCount;
		private const int _EXPORTED_ITEMS_UPDATE_THRESHOLD = 1000;
		private readonly IMessageService _messageService;
		private readonly IJobHistoryErrorService _historyErrorService;
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IIntegrationPointProviderTypeService _integrationPointProviderTypeService;
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly DateTime _startTime;

		#endregion //Fields

		#region Events

		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit { add { } remove { } }
		public event BatchCreated OnBatchCreate { add { } remove { } }
		public event StatusUpdate OnStatusUpdate;
		public event JobError OnJobError { add { } remove { } }
		public event RowError OnDocumentError;
		public event StatisticsUpdate OnStatisticsUpdate { add { } remove { } }

		#endregion //Events

		#region Methods

		public StatisticsLoggingMediator(IMessageService messageService, IJobHistoryErrorService historyErrorService,
			ICaseServiceContext caseServiceContext,
			IIntegrationPointProviderTypeService integrationPointProviderTypeService, 
			IDateTimeHelper dateTimeHelper)
		{
			_messageService = messageService;
			_historyErrorService = historyErrorService;
			_caseServiceContext = caseServiceContext;
			_integrationPointProviderTypeService = integrationPointProviderTypeService;
			_dateTimeHelper = dateTimeHelper;
			_startTime = _dateTimeHelper.Now();
		}

		public void RegisterEventHandlers(IUserMessageNotification userMessageNotification,
			ICoreExporterStatusNotification exporterStatusNotification)
		{
			exporterStatusNotification.OnBatchCompleted += OnOnBatchCompleteChanged;
			exporterStatusNotification.StatusMessage += OnStatusMessageChanged;
			exporterStatusNotification.StatusMessage += OnDocumentErrorChnaged;
		}

		private void OnDocumentErrorChnaged(ExportEventArgs exportArgs)
		{
			// Here we track document error level
			if (exportArgs.EventType == EventType.Error)
			{
				OnDocumentError?.Invoke(exportArgs.Message, exportArgs.Message);
			}
		}

		private void OnStatusMessageChanged(ExportEventArgs exportArgs)
		{
			// RDC firse many events even exported items count has not been chnaged. We need to filter it out
			if (CanUpdateJobStatus(exportArgs))
			{
				_currentExportedItemChunkCount += exportArgs.DocumentsExported;
				OnStatusUpdate?.Invoke(_EXPORTED_ITEMS_UPDATE_THRESHOLD, 0);
			}

			if (CanSendLiveStatistics(exportArgs))
			{
				SendLiveStatistics(exportArgs);
			}

			if (CanSendEndStatistics(exportArgs))
			{
				SendEndStatistics(exportArgs);
			}
		}

		private void SendLiveStatistics(ExportEventArgs e)
		{
			var message = new JobApmThroughputMessage();
			BuildJobApmMessageBase(message);
			message.FileThroughput = e.Statistics.FileThroughput;
			message.MetadataThroughput = e.Statistics.MetadataThroughput;
			_messageService.Send(message);
		}
			_messageService.Send(jobThroughputBytesMessage);
		}
		
		private void BuildJobApmMessageBase(JobMessageBase m)
		{
			string provider = GetProviderName();
			m.Provider = provider;
			m.CorrelationID = _historyErrorService.JobHistory.BatchInstance;
			m.UnitOfMeasure = "Byte(s)";
			m.JobID = _historyErrorService.JobHistory.JobID;
			m.WorkspaceID = _caseServiceContext.WorkspaceID;
		}

		private void SendEndStatistics(ExportEventArgs e)
		{
			var message = new ExportJobStatisticsMessage();
			BuildJobApmMessageBase(message);
			message.FileBytes = e.Statistics.FileBytes;
			message.MetaBytes = e.Statistics.MetadataBytes;
			_messageService.Send(message);
		}

		private void BuildJobApmMessageBase(JobApmMessageBase m)
		{
			string provider = GetProviderName();
			m.Provider = provider;
			m.CorellationID = _historyErrorService.JobHistory.BatchInstance;
			m.UnitOfMeasure = "Byte(s)";
			m.JobID = _historyErrorService.JobHistory.JobID;
			m.WorkspaceID = _caseServiceContext.WorkspaceID;
			m.CustomData["Provider"] = provider;
		}

		private string GetProviderName()
		{
			return _historyErrorService.IntegrationPoint.GetProviderType(_providerTypeService).ToString();
		}

		private bool CanUpdateJobStatus(ExportEventArgs exportArgs)
		{
			return exportArgs.EventType == EventType.Progress
				&& (exportArgs.DocumentsExported % _EXPORTED_ITEMS_UPDATE_THRESHOLD) == 0
				&& exportArgs.DocumentsExported != _currentExportedItemChunkCount;
		}

		private bool CanSendLiveStatistics(ExportEventArgs e)
		{
			return e.EventType == EventType.Statistics;
		}

		private bool CanSendEndStatistics(ExportEventArgs e)
		{
			return e.EventType == EventType.End;
		}

		private void OnOnBatchCompleteChanged(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount)
		{
			OnBatchComplete?.Invoke(startTime, endTime, totalRows, errorRowCount);
		}

		#endregion //Methods
	}
}
