using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ADF;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
    internal class NonDocumentSynchronizationExecutor : SynchronizationExecutorBase<INonDocumentSynchronizationConfiguration>
    {
        public NonDocumentSynchronizationExecutor(
            IImportJobFactory importJobFactory,
            IBatchRepository batchRepository,
            IJobProgressHandlerFactory jobProgressHandlerFactory,
            IFieldManager fieldManager,
            IFieldMappings fieldMappings,
            IJobStatisticsContainer jobStatisticsContainer,
            IJobCleanupConfiguration jobCleanupConfiguration,
            IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
            Func<IStopwatch> stopwatchFactory,
            ISyncMetrics syncMetrics,
            IAPILog logger,
            IADLSUploader uploader,
            IUserContextConfiguration userContextConfiguration,
            IADFTransferEnabler adfTransferEnabler) : base(
            importJobFactory,
            BatchRecordType.NonDocuments,
            batchRepository,
            jobProgressHandlerFactory,
            fieldManager,
            fieldMappings,
            jobStatisticsContainer,
            jobCleanupConfiguration,
            automatedWorkflowTriggerConfiguration,
            stopwatchFactory,
            syncMetrics,
            userContextConfiguration,
            uploader,
            adfTransferEnabler,
            logger)
        {
        }

        protected override Task<IImportJob> CreateImportJobAsync(INonDocumentSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
        {
            return ImportJobFactory.CreateRdoImportJobAsync(configuration, batch, token);
        }

        protected override void UpdateImportSettings(INonDocumentSynchronizationConfiguration configuration)
        {
            configuration.IdentityFieldId = GetDestinationIdentityFieldId();
        }

        protected override void ChildReportBatchMetrics(int batchId, BatchProcessResult batchProcessResult, TimeSpan batchTime, TimeSpan importApiTimer)
        {
            SyncMetrics.Send(new NonDocumentBatchEndMetric()
            {
                TotalRecordsRequested = batchProcessResult.TotalRecordsRequested,
                TotalRecordsTransferred = batchProcessResult.TotalRecordsTransferred,
                TotalRecordsFailed = batchProcessResult.TotalRecordsFailed,
                BytesMetadataTransferred = batchProcessResult.MetadataBytesTransferred,
                BytesTransferred = batchProcessResult.BytesTransferred,
                BatchImportAPITime = importApiTimer.TotalMilliseconds,
                BatchTotalTime = batchTime.TotalMilliseconds,
            });
        }

        protected override Task<TaggingExecutionResult> TagObjectsAsync(IImportJob importJob, ISynchronizationConfiguration configuration, CompositeCancellationToken token)
        {
            var dummyResult = TaggingExecutionResult.Success();
            dummyResult.TaggedDocumentsCount = 0;

            return Task.FromResult(dummyResult);
        }
    }
}
