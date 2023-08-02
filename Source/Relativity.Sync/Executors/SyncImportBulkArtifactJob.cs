using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using kCura.Relativity.DataReaderClient;
using Relativity.AntiMalware.SDK;
using Relativity.API;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
    [ExcludeFromCodeCoverage]
    internal sealed class SyncImportBulkArtifactJob : ISyncImportBulkArtifactJob
    {
        private const string _IAPI_DOCUMENT_IDENTIFIER_COLUMN = "Identifier";
        private const string _IAPI_IMAGE_IDENTIFIER_COLUMN = "DocumentID";
        private const string _IAPI_MESSAGE_COLUMN = "Message";
        private const string _IAPI_MALWARE_COLUMN = "Malware";

        private readonly IAntiMalwareEventHelper _antiMalwareEventHelper;
        private readonly IImportBulkArtifactJob _importBulkArtifactJob;
        private readonly int _workspaceId;
        private readonly IAPILog _logger;

        private int _sourceWorkspaceErrorItemsCount = 0;

        public SyncImportBulkArtifactJob(
            ImportBulkArtifactJob importBulkArtifactJob,
            ISourceWorkspaceDataReader sourceWorkspaceDataReader,
            IAntiMalwareEventHelper antiMalwareEventHelper,
            int workspaceId,
            IAPILog logger)
            : this(
                sourceWorkspaceDataReader,
                antiMalwareEventHelper,
                workspaceId,
                logger)
        {
            importBulkArtifactJob.OnProgress += RaiseOnProgress;
            importBulkArtifactJob.OnError += HandleIapiDocumentItemLevelError;
            importBulkArtifactJob.OnError += HandleMalware;
            importBulkArtifactJob.OnComplete += HandleIapiJobComplete;
            importBulkArtifactJob.OnFatalException += HandleIapiFatalException;

            _importBulkArtifactJob = importBulkArtifactJob;
        }

        public SyncImportBulkArtifactJob(
            ImageImportBulkArtifactJob imageImportBulkArtifactJob,
            ISourceWorkspaceDataReader sourceWorkspaceDataReader,
            IAntiMalwareEventHelper antiMalwareEventHelper,
            int workspaceId,
            IAPILog logger)
            : this(
                sourceWorkspaceDataReader,
                antiMalwareEventHelper,
                workspaceId,
                logger)
        {
            imageImportBulkArtifactJob.OnProgress += RaiseOnProgress;
            imageImportBulkArtifactJob.OnError += HandleIapiImageItemLevelError;
            imageImportBulkArtifactJob.OnError += HandleMalware;
            imageImportBulkArtifactJob.OnComplete += HandleIapiJobComplete;
            imageImportBulkArtifactJob.OnFatalException += HandleIapiFatalException;

            _importBulkArtifactJob = imageImportBulkArtifactJob;
        }

        private SyncImportBulkArtifactJob(ISourceWorkspaceDataReader sourceWorkspaceDataReader, IAntiMalwareEventHelper antiMalwareEventHelper, int workspaceId, IAPILog logger)
        {
            _antiMalwareEventHelper = antiMalwareEventHelper;
            _workspaceId = workspaceId;
            _logger = logger.ForContext<SyncImportBulkArtifactJob>();
            ItemStatusMonitor = sourceWorkspaceDataReader.ItemStatusMonitor;
            sourceWorkspaceDataReader.OnItemReadError += HandleSourceWorkspaceDataItemReadError;
        }

        public event SyncJobEventHandler<ItemLevelError> OnItemLevelError;

        public event SyncJobEventHandler<ImportApiJobProgress> OnProgress;

        public event SyncJobEventHandler<ImportApiJobStatistics> OnComplete;

        public event SyncJobEventHandler<ImportApiJobStatistics> OnFatalException;

        public IItemStatusMonitor ItemStatusMonitor { get; }

        public void Execute()
        {
            _importBulkArtifactJob.Execute();
        }

        private void RaiseOnProgress(long completedRow)
        {
            OnProgress?.Invoke(new ImportApiJobProgress(completedRow));
        }

        private void HandleIapiDocumentItemLevelError(IDictionary row)
        {
            RaiseOnItemLevelError(
                CreateItemLevelError(
                    row,
                    _IAPI_DOCUMENT_IDENTIFIER_COLUMN,
                    _IAPI_MESSAGE_COLUMN));
        }

        private void HandleIapiImageItemLevelError(IDictionary row)
        {
            RaiseOnItemLevelError(
                CreateItemLevelError(
                    row,
                    _IAPI_IMAGE_IDENTIFIER_COLUMN,
                    _IAPI_MESSAGE_COLUMN));
        }

        private void HandleMalware(IDictionary row)
        {
            string infectedFilePath = GetValueOrNull(row, _IAPI_MALWARE_COLUMN);
            if (!string.IsNullOrWhiteSpace(infectedFilePath))
            {
                try
                {
                    string documentIdentifier = GetValueOrNull(row, _IAPI_DOCUMENT_IDENTIFIER_COLUMN) ??
                                                GetValueOrNull(row, _IAPI_IMAGE_IDENTIFIER_COLUMN) ??
                                                string.Empty;

                    _logger.LogWarning("Malware detected in document: {documentIdentifier}", documentIdentifier);

                    AntiMalwareEvent antiMalwareEvent = new AntiMalwareEvent
                    {
                    };

                    _antiMalwareEventHelper.ReportAntiMalwareEventAsync(antiMalwareEvent).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred when trying to report anti-malware event.");
                }
            }
        }

        private void HandleIapiJobComplete(JobReport jobReport)
        {
            OnComplete?.Invoke(CreateJobStatistics(jobReport));
        }

        private void HandleIapiFatalException(JobReport jobReport)
        {
            OnFatalException?.Invoke(CreateJobStatistics(jobReport));
        }

        private ItemLevelError CreateItemLevelError(IDictionary rawItemLevelError, string identifierColumnName, string messageColumnName)
        {
            return new ItemLevelError(
                GetValueOrNull(rawItemLevelError, identifierColumnName),
                $"IAPI {GetValueOrNull(rawItemLevelError, messageColumnName)}");
        }

        private void RaiseOnItemLevelError(ItemLevelError itemLevelError)
        {
            OnItemLevelError?.Invoke(itemLevelError);
        }

        private void HandleSourceWorkspaceDataItemReadError(long completedItem, ItemLevelError itemLevelError)
        {
            _sourceWorkspaceErrorItemsCount++;

            RaiseOnProgress(completedItem);
            RaiseOnItemLevelError(itemLevelError);
        }

        private ImportApiJobStatistics CreateJobStatistics(JobReport jobReport)
        {
            ImportApiJobStatistics statistics = new ImportApiJobStatistics(
                jobReport.TotalRows + _sourceWorkspaceErrorItemsCount,
                jobReport.ErrorRowCount + _sourceWorkspaceErrorItemsCount,
                jobReport.MetadataBytes,
                jobReport.FileBytes,
                jobReport.FatalException);

            return statistics;
        }

        private string GetValueOrNull(IDictionary row, string key)
        {
            return row.Contains(key) ? row[key].ToString() : null;
        }
    }
}
