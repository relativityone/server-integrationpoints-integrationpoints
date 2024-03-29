﻿using System;
using kCura.IntegrationPoints.Common.Monitoring;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Utils;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public class JobStatisticsService : IJobStatisticsService
    {
        private static object _lockToken = new object();
        private readonly IMessageService _messageService;
        private readonly IIntegrationPointProviderTypeService _integrationPointProviderTypeService;
        private readonly IDiagnosticLog _diagnosticLog;
        private readonly ITaskParameterHelper _helper;
        private readonly IJobStatisticsQuery _query;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IAPILog _logger;
        private readonly IFileSizesStatisticsService _fileSizeStatisticsService;
        private Job _job;
        private int _rowErrors;

        public JobStatisticsService(
            IJobStatisticsQuery query,
            ITaskParameterHelper taskParameterHelper,
            IJobHistoryService jobHistoryService,
            IWorkspaceDBContext context,
            IHelper helper,
            IFileSizesStatisticsService fileSizesStatisticsStatisticsService,
            IMessageService messageService,
            IIntegrationPointProviderTypeService integrationPointProviderTypeService,
            IDiagnosticLog diagnosticLog)
        {
            _query = query;
            _helper = taskParameterHelper;
            _jobHistoryService = jobHistoryService;
            _messageService = messageService;
            _integrationPointProviderTypeService = integrationPointProviderTypeService;
            _diagnosticLog = diagnosticLog;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<JobStatisticsService>();

            _fileSizeStatisticsService = fileSizesStatisticsStatisticsService;
        }

        /// <summary>
        /// This constructor is used in our unit tests
        /// </summary>
        internal JobStatisticsService()
        {
        }

        private SourceConfiguration IntegrationPointSourceConfiguration { get; set; }

        private ImportSettings IntegrationPointImportSettings { get; set; }

        public void Subscribe(IBatchReporter reporter, Job job)
        {
            _job = job;

            if (reporter != null)
            {
                _diagnosticLog.LogDiagnostic("Subscribe reporting for Job {jobId}", job?.JobId);

                reporter.OnStatusUpdate += StatusUpdate;
                reporter.OnStatisticsUpdate += OnStatisticsUpdate;
                reporter.OnBatchComplete += OnJobComplete;
                reporter.OnDocumentError += RowError;
            }
        }

        public void SetIntegrationPointConfiguration(ImportSettings importSettings, SourceConfiguration sourceConfiguration)
        {
            IntegrationPointSourceConfiguration = sourceConfiguration;
            IntegrationPointImportSettings = importSettings;
        }

        public void Update(Guid identifier, int transferredItem, int erroredCount)
        {
            lock (_lockToken)
            {
                _diagnosticLog.LogDiagnostic(
                    "Updating... Identifier: {identifier}, TransferredItemsCount: {itemsCount}, ErrorItemsCount: {errorsCount}",
                    transferredItem,
                    erroredCount);
                UpdateJobHistory(transferredItem, erroredCount);
            }
        }

        private void OnStatisticsUpdate(double metadataThroughput, double fileThroughput)
        {
            string provider = GetProviderType(_job).ToString();

            var message = new JobProgressMessage
            {
                Provider = provider,
                CorrelationID = _helper.GetBatchInstance(_job).ToString(),
                UnitOfMeasure = UnitsOfMeasureConstants.BYTES,
                JobID = _job.JobId.ToString(),
                WorkspaceID = ((IntegrationPointSourceConfiguration?.SourceWorkspaceArtifactId == 0)
                    ? IntegrationPointImportSettings?.CaseArtifactId
                    : IntegrationPointSourceConfiguration?.SourceWorkspaceArtifactId).GetValueOrDefault(),
                MetadataThroughput = metadataThroughput,
                FileThroughput = fileThroughput,
                CustomData = { ["Provider"] = provider }
            };

            _messageService.Send(message);
        }

        private void RowError(string documentIdentifier, string errorMessage)
        {
            _rowErrors++;
        }

        private void OnJobComplete(DateTime start, DateTime end, int total, int errorCount)
        {
            _logger.LogInformation("OnJobComplete event started for JobID={jobId}", _job.JobId);

            Guid batchInstance = _helper.GetBatchInstance(_job);
            string tableName = JobTracker.GenerateJobTrackerTempTableName(_job, batchInstance.ToString());

            long totalSize;
            if (_job.TaskType == TaskType.ImportService.ToString())
            {
                totalSize = 0; // No way to get this from JobMessageBase and reading it from Db will be to expensive
            }
            else
            {
                totalSize = _fileSizeStatisticsService.CalculatePushedFilesSizeForJobHistory((int)_job.JobId, IntegrationPointImportSettings, IntegrationPointSourceConfiguration);
            }

            lock (_lockToken)
            {
                // TODO refactoring, command query separation
                _logger.LogInformation("In JobStatisticsService.cs executing UpdateAndRetrieveStats()[CreateJobTrackingEntry.sql and UpdateJobStatistics.sql] with {tableName}", tableName);
                JobStatistics stats = _query.UpdateAndRetrieveStats(tableName, _job.JobId, new JobStatistics { Completed = total, Errored = _rowErrors, ImportApiErrors = errorCount }, _job.WorkspaceID);
                UpdateJobHistory(batchInstance, stats, totalSize);
            }

            _rowErrors = 0;
        }

        private void StatusUpdate(int importedCount, int errorCount)
        {
            _diagnosticLog.LogDiagnostic("Status Update: ImportedDocsCount {importedDocsCount}, ErroredDocsCount {erroredDocsCount}", importedCount, errorCount);

            Update(_helper.GetBatchInstance(_job), importedCount, errorCount);
        }

        private void UpdateJobHistory(Guid batchInstance, JobStatistics stats, long totalSize)
        {
            try
            {
                Data.JobHistory historyRdo = _jobHistoryService.GetRdoWithoutDocuments(batchInstance);
                int updatedNumberOfTransferredItems = stats.Imported > 0 ? stats.Imported : 0;
                historyRdo.ItemsTransferred = updatedNumberOfTransferredItems;
                historyRdo.ItemsWithErrors = stats.ImportApiErrors;
                historyRdo.FilesSize = FileSizeUtils.FormatFileSize(totalSize);
                historyRdo.ItemsRead = updatedNumberOfTransferredItems;

                _diagnosticLog.LogDiagnostic(
                    "Updating JobHistory RDO - ItemsTransferred: {itemsTransferred}, ItemsRead: {itemsRead}, ItemsWithErrors: {itemsWithErrors}, FilesSize: {filesSize}",
                    historyRdo.ItemsTransferred,
                    historyRdo.ItemsRead,
                    historyRdo.ItemsWithErrors,
                    historyRdo.FilesSize);

                _jobHistoryService.UpdateRdoWithoutDocuments(historyRdo);
            }
            catch (Exception e)
            {
                LogUpdatingStatisticsError(e);
            }
        }

        private void UpdateJobHistory(int transferredItem, int erroredCount)
        {
            try
            {
                Data.JobHistory historyRdo = _jobHistoryService.GetRdoWithoutDocuments(_helper.GetBatchInstance(_job));
                int updatedNumberOfTransferredItems = Math.Max(0, (historyRdo.ItemsTransferred ?? 0) + transferredItem);
                historyRdo.ItemsTransferred = updatedNumberOfTransferredItems;
                historyRdo.ItemsWithErrors += erroredCount;
                historyRdo.ItemsRead = updatedNumberOfTransferredItems;

                _diagnosticLog.LogDiagnostic(
                    "Updating JobHistory RDO - ItemsTransferred: {itemsTransferred}, ItemsRead: {itemsRead}, ItemsWithErrors: {itemsWithErrors}, FilesSize: {filesSize}",
                    historyRdo.ItemsTransferred,
                    historyRdo.ItemsRead,
                    historyRdo.ItemsWithErrors,
                    historyRdo.FilesSize);

                _jobHistoryService.UpdateRdoWithoutDocuments(historyRdo);
            }
            catch (Exception e)
            {
                LogUpdatingStatisticsError(e);
            }
        }

        private ProviderType GetProviderType(Job job)
        {
            return _integrationPointProviderTypeService.GetProviderType(job.RelatedObjectArtifactID);
        }

        #region Logging

        private void LogUpdatingStatisticsError(Exception e)
        {
            _logger.LogError(e, "Error occurred during Job Statistics update.");
        }

        #endregion
    }
}
