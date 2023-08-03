using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;
using Exception = System.Exception;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
    public class ImportService : IImportService, IBatchReporter
    {
        private const int _JOB_PROGRESS_TIMEOUT_MILLISECONDS = 5000;
        private const string _IMPORT_API_ERROR_PREFIX = "IAPI";
        private readonly BatchManager _batchManager;
        private readonly Dictionary<string, int> _inputMappings;
        private readonly IAPILog _logger;
        private readonly IHelper _helper;
        private readonly IJobStopManager _jobStopManager;
        private readonly IDiagnosticLog _diagnosticLog;
        private readonly IImportApiFactory _factory;
        private readonly IImportJobFactory _jobFactory;
        private readonly JobProgressInfo _jobProgressInfo = new JobProgressInfo();
        private readonly NativeFileImportService _nativeFileImportService;
        private Dictionary<int, string> _idToFieldNameDictionary;
        private IImportAPI _importApi;
        private int _lastJobStatusUpdate;
        private int _totalRowsImported;
        private int _totalRowsWithErrors;

        public ImportService(
            ImportSettings settings,
            Dictionary<string, int> fieldMappings,
            BatchManager batchManager,
            NativeFileImportService nativeFileImportService,
            IImportApiFactory factory,
            IImportJobFactory jobFactory,
            IHelper helper,
            IJobStopManager jobStopManager,
            IDiagnosticLog diagnosticLog)
        {
            _helper = helper;
            _jobStopManager = jobStopManager;
            _diagnosticLog = diagnosticLog;
            Settings = settings;
            _batchManager = batchManager;
            _inputMappings = fieldMappings;
            _nativeFileImportService = nativeFileImportService;
            _factory = factory;
            _jobFactory = jobFactory;
            _logger = _helper.GetLoggerFactory().GetLogger().ForContext<ImportService>();

            if (_batchManager != null)
            {
                _batchManager.OnBatchCreate += ImportService_OnBatchCreate;
            }
        }

        private Dictionary<string, string> FieldMappings { get; set; }

        public event StatusUpdate OnStatusUpdate;

        public event BatchCompleted OnBatchComplete;

        public event BatchSubmitted OnBatchSubmit;

        public event BatchCreated OnBatchCreate;

        public event StatisticsUpdate OnStatisticsUpdate;

        public event JobError OnJobError;

        public event RowError OnDocumentError;

        public ImportSettings Settings { get; }

        public virtual void Initialize()
        {
            if (_importApi == null)
            {
                LogInitializingImportApi();
                Connect();
                SetupFieldDictionary();
                Dictionary<string, int> fieldMapping = _inputMappings;
                FieldMappings = ValidateAllMappedFieldsAreInWorkspace(fieldMapping, _idToFieldNameDictionary);
                HashSet<string> columnNames = new HashSet<string>();
                FieldMappings.Values.ToList().ForEach(x =>
                {
                    if (!columnNames.Contains(x))
                    {
                        columnNames.Add(x);
                    }
                });

                _batchManager.ColumnNames = columnNames;
            }
        }

        public void AddRow(Dictionary<string, object> sourceFields)
        {
            Dictionary<string, object> importFields = GenerateImportFields(sourceFields, FieldMappings);
            if (importFields.Count > 0)
            {
                _batchManager.Add(importFields);
                PushBatchIfFull(false);
            }
        }

        public bool PushBatchIfFull(bool forcePush)
        {
            bool isFull = _batchManager.IsBatchFull();
            if (isFull || forcePush)
            {
                LogPushingBatch(forcePush);
                try
                {
                    IDataReader sourceData = _batchManager.GetBatchData();
                    if (sourceData != null)
                    {
                        KickOffImport(new DefaultTransferContext(new PausableDataReader(sourceData, _jobStopManager)));
                    }
                    else
                    {
                        CompleteBatch(DateTime.Now, DateTime.Now, 0, 0);
                    }
                }
                finally
                {
                    _batchManager.ClearDataSource();
                }
            }
            return isFull;
        }

        public virtual void KickOffImport(IDataTransferContext context)
        {
            IJobImport importJob = _jobFactory.Create(_importApi, Settings, context, _helper);

            _totalRowsImported = context.TransferredItemsCount;
            _totalRowsWithErrors = context.FailedItemsCount;

            // Assign events
            importJob.OnComplete += ImportJob_OnComplete;
            importJob.OnFatalException += ImportJob_OnComplete;
            importJob.OnError += ImportJob_OnError;
            importJob.OnMessage += ImportJob_OnMessage;
            importJob.OnProgress += ImportJob_OnProgress;
            importJob.OnProcessProgress += ImportJob_OnProcessProgress;
            ImportService_OnBatchSubmit(_batchManager.CurrentSize, _batchManager.MinimumBatchSize);

            LogImportJobStarted();
            importJob.Execute();
        }

        public Dictionary<string, string> ValidateAllMappedFieldsAreInWorkspace(Dictionary<string, int> fieldMapping, Dictionary<int, string> rdoAllFields)
        {
            Dictionary<string, string> mapping = new Dictionary<string, string>();

            List<int> missingFields = new List<int>();
            foreach (string mapSourceFieldName in fieldMapping.Keys)
            {
                int mapRdoFieldId = fieldMapping[mapSourceFieldName];
                if (!rdoAllFields.ContainsKey(mapRdoFieldId))
                {
                    missingFields.Add(mapRdoFieldId);
                }
                else
                {
                    if (!mapping.ContainsKey(mapSourceFieldName))
                    {
                        mapping.Add(mapSourceFieldName, rdoAllFields[mapRdoFieldId]);
                    }
                }
            }
            if (missingFields.Count > 0)
            {
                string missingFieldFormatted = string.Join(", ", missingFields);
                var message = $"Missing mapped field IDs: {missingFieldFormatted}";
                LogFieldInWorkspaceValidationError(message);
                throw new Exception(message);
            }
            return mapping;
        }

        public int TotalRowsProcessed => _totalRowsImported;

        private void Connect()
        {
            _importApi = _factory.GetImportAPI();
        }

        internal void SetupFieldDictionary()
        {
            try
            {
                IImportApiFacade facade = _factory.GetImportApiFacade();
                _idToFieldNameDictionary = facade.GetWorkspaceFieldsNames(Settings.DestinationConfiguration.CaseArtifactId, Settings.DestinationConfiguration.ArtifactTypeId);
            }
            catch (Exception e)
            {
                LogSettingFieldDictionaryError(e);
                throw;
            }
        }

        internal Dictionary<string, object> GenerateImportFields(Dictionary<string, object> sourceFields, Dictionary<string, string> mapping)
        {
            Dictionary<string, object> importFields = new Dictionary<string, object>();

            foreach (string sourceFieldId in sourceFields.Keys)
            {
                if (mapping.ContainsKey(sourceFieldId))
                {
                    string rdoFieldName = mapping[sourceFieldId];

                    if (!importFields.ContainsKey(rdoFieldName))
                    {
                        importFields.Add(rdoFieldName, sourceFields[sourceFieldId]);
                    }
                }
            }
            if ((_nativeFileImportService != null) && _nativeFileImportService.ImportNativeFiles)
            {
                importFields.Add(_nativeFileImportService.DestinationFieldName, sourceFields[_nativeFileImportService.SourceFieldName]);
            }
            return importFields;
        }

        internal string PrependString(string prefix, string message)
        {
            message = string.IsNullOrWhiteSpace(message) ? "[Unknown message]" : message;
            return $"{prefix} {message}";
        }

        private void ImportService_OnBatchCreate(int batchSize)
        {
            LogOnBatchCreateEvent(batchSize);
            OnBatchCreate?.Invoke(batchSize);
        }

        private void ImportService_OnBatchSubmit(int currentSize, int minSize)
        {
            OnBatchSubmit?.Invoke(currentSize, minSize);
        }

        private void ImportJob_OnComplete(JobReport jobReport)
        {
            LogOnCompleteEvent();
            SaveDocumentsError(jobReport.ErrorRows);

            if (jobReport.FatalException != null)
            {
                Exception fatalException = jobReport.FatalException;

                LogOnErrorEvent(fatalException);
                OnJobError?.Invoke(fatalException);

                CompleteBatch(jobReport.StartTime, jobReport.EndTime, 0, 0);

                throw new IntegrationPointsException("Fatal Exception in Import API", fatalException)
                {
                    ShouldAddToErrorsTab = true,
                    ExceptionSource = IntegrationPointsExceptionSource.IAPI
                };
            }

            CompleteBatch(jobReport.StartTime, jobReport.EndTime, jobReport.TotalRows, jobReport.ErrorRowCount);
        }

        private void SaveDocumentsError(IList<JobReport.RowError> errors)
        {
            if (OnDocumentError != null)
            {
                foreach (JobReport.RowError error in errors)
                {
                    OnDocumentError(error.Identifier, PrependString(_IMPORT_API_ERROR_PREFIX, error.Message));
                }
            }
        }

        private void CompleteBatch(DateTime start, DateTime end, int processedRows, int errorRows)
        {
            LogBatchCompleted(start, end, processedRows, errorRows);

            Interlocked.Add(ref _totalRowsImported, processedRows);
            Interlocked.Add(ref _totalRowsWithErrors, errorRows);

            OnBatchComplete?.Invoke(start, end, processedRows, errorRows);
        }

        private void ImportJob_OnMessage(Status status)
        {
            LogOnMessageEvent(status);
        }

        private void ImportJob_OnError(System.Collections.IDictionary row)
        {
            _jobProgressInfo.ItemErrored();
            UpdateStatus();
        }

        private void ImportJob_OnProgress(long item)
        {
            _jobProgressInfo.ItemTransferred();

            _diagnosticLog.LogDiagnostic("ImportJob_OnProgress - Item: {item}, JobProgressInfo: {@jobProgressInfo} ", item, _jobProgressInfo);

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (!_jobProgressInfo.IsValid())
            {
                return;
            }

            if (OnStatusUpdate == null)
            {
                return;
            }

            if (Environment.TickCount - _lastJobStatusUpdate > _JOB_PROGRESS_TIMEOUT_MILLISECONDS)
            {
                _diagnosticLog.LogDiagnostic(
                    "OnStatusUpdate - ItemsTransferred: {transferredItemsCount}, ItemsErrored: {itemsErrored}",
                    _jobProgressInfo.NumberOfItemsTransferred,
                    _jobProgressInfo.NumberOfItemsErrored);

                OnStatusUpdate(_jobProgressInfo.NumberOfItemsTransferred, _jobProgressInfo.NumberOfItemsErrored);

                _lastJobStatusUpdate = Environment.TickCount;
                _jobProgressInfo.Reset();

                _diagnosticLog.LogDiagnostic("Status was updated.");
            }
        }

        private void ImportJob_OnProcessProgress(FullStatus processStatus)
        {
            OnStatisticsUpdate?.Invoke(processStatus.MetadataThroughput, processStatus.FilesThroughput);
        }

        #region Logging

        private void LogInitializingImportApi()
        {
            _logger.LogInformation("Attempting to initialize Import API.");
        }

        private void LogPushingBatch(bool forcePush)
        {
            _logger.LogInformation("Attempting to push batch (ForcePush: {ForcePush}).", forcePush);
        }

        private void LogImportJobStarted()
        {
            _logger.LogInformation("Import Job started.");
        }

        private void LogSettingFieldDictionaryError(Exception e)
        {
            _logger.LogError(e, "Failed to setup field dictionary for Import API.");
        }

        private void LogFieldInWorkspaceValidationError(string message)
        {
            _logger.LogError("Not all fields have been found in workspace. {Message}.", message);
        }

        private void LogOnBatchCreateEvent(int batchSize)
        {
            _logger.LogInformation("ImportService OnBatchCreate event received with batch size: {BatchSize}.", batchSize);
        }

        private void LogOnErrorEvent(Exception fatalException)
        {
            _logger.LogInformation(fatalException, "ImportJob returned FatalException.");
        }

        private void LogOnCompleteEvent()
        {
            _logger.LogInformation("ImportJob OnComplete event received.");
        }

        private void LogBatchCompleted(DateTime start, DateTime end, int totalRows, int errorRows)
        {
            _logger.LogInformation(
                "Batch completed. Total rows: {TotalRows}. Rows with error: {ErrorRows}. Time in milliseconds: {Time}.",
                totalRows,
                errorRows,
                (end - start).Milliseconds);
        }

        private void LogOnMessageEvent(Status status)
        {
            _diagnosticLog.LogDiagnostic("ImportJob OnMessage event received. Current status: {Status}.", status?.Message);
        }

        #endregion
    }
}
