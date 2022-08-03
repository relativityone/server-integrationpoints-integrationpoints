using System;
using kCura.IntegrationPoints.Common.Monitoring;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using Relativity.DataExchange.Process;
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
        public event BatchSubmitted OnBatchSubmit;
        public event BatchCreated OnBatchCreate;
        public event StatusUpdate OnStatusUpdate;
        public event JobError OnJobError;
        public event RowError OnDocumentError;
        public event StatisticsUpdate OnStatisticsUpdate;

        #endregion //Events

        #region Methods

        public StatisticsLoggingMediator(
            IMessageService messageService, 
            IJobHistoryErrorService historyErrorService,
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
            InitializeEmptyEventHandlers();
        }

        private void InitializeEmptyEventHandlers()
        {
            OnBatchSubmit = (size, batchSize) => {};
            OnBatchCreate = size => { };
            OnJobError = exception => { };
            OnStatisticsUpdate = (throughput, fileThroughput) => { };
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
            if (exportArgs.EventType == EventType2.Error)
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
            var message = new JobProgressMessage();
            BuildJobApmMessageBase(message);
            message.FileThroughput = e.Statistics.FileTransferThroughput;
            message.MetadataThroughput = e.Statistics.MetadataTransferThroughput;
            _messageService.Send(message);
        }

        private void SendEndStatistics(ExportEventArgs e)
        {
            long jobSizeInBytes = e.Statistics.FileTransferredBytes + e.Statistics.MetadataTransferredBytes;
            TimeSpan duration = _dateTimeHelper.Now() - _startTime;
            double bytesPerSecond = duration.TotalSeconds > 0 ? jobSizeInBytes / duration.TotalSeconds : 0;

            SendJobStatisticsMessage(e.Statistics.FileTransferredBytes, e.Statistics.MetadataTransferredBytes);
            SendJobThroughputBytesMessage(bytesPerSecond);
        }

        private void SendJobStatisticsMessage(long fileBytes, long metaBytes)
        {
            var jobStatisticsMessage = new JobStatisticsMessage();
            BuildJobApmMessageBase(jobStatisticsMessage);
            jobStatisticsMessage.FileBytes = fileBytes;
            jobStatisticsMessage.MetaBytes = metaBytes;
            jobStatisticsMessage.JobSizeInBytes = fileBytes + metaBytes;
            _messageService.Send(jobStatisticsMessage);
        }

        private void SendJobThroughputBytesMessage(double bytesPerSecond)
        {
            string providerName = GetProviderName();
            var jobThroughputBytesMessage = new JobThroughputBytesMessage()
            {
                Provider = providerName,
                CorrelationID = _historyErrorService.JobHistory.BatchInstance,
                UnitOfMeasure = UnitsOfMeasureConstants.BYTES,
                WorkspaceID = _caseServiceContext.WorkspaceID,
                JobID = _historyErrorService.JobHistory.JobID,
                BytesPerSecond = bytesPerSecond
            };
            _messageService.Send(jobThroughputBytesMessage);
        }

        private void BuildJobApmMessageBase(JobMessageBase m)
        {
            string provider = GetProviderName();
            m.Provider = provider;
            m.CorrelationID = _historyErrorService.JobHistory.BatchInstance;
            m.UnitOfMeasure = UnitsOfMeasureConstants.BYTES;
            m.JobID = _historyErrorService.JobHistory.JobID;
            m.WorkspaceID = _caseServiceContext.WorkspaceID;
        }

        private string GetProviderName()
        {
            return _integrationPointProviderTypeService.GetProviderType(_historyErrorService.IntegrationPoint).ToString();    
        }

        private bool CanUpdateJobStatus(ExportEventArgs exportArgs)
        {
            return exportArgs.EventType == EventType2.Progress
                && (exportArgs.DocumentsExported % _EXPORTED_ITEMS_UPDATE_THRESHOLD) == 0
                && exportArgs.DocumentsExported != _currentExportedItemChunkCount;
        }

        private bool CanSendLiveStatistics(ExportEventArgs e)
        {
            return e.EventType == EventType2.Statistics;
        }

        private bool CanSendEndStatistics(ExportEventArgs e)
        {
            return e.EventType == EventType2.End;
        }

        private void OnOnBatchCompleteChanged(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount)
        {
            OnBatchComplete?.Invoke(startTime, endTime, totalRows, errorRowCount);
        }

        #endregion //Methods
    }
}
