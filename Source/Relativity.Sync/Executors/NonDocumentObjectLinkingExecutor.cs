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
    internal class NonDocumentObjectLinkingExecutor : SynchronizationExecutorBase<INonDocumentObjectLinkingConfiguration>
    {
        public NonDocumentObjectLinkingExecutor(
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
            IUserContextConfiguration userContextConfiguration,
            IADLSUploader uploader,
            IAPILog logger,
            IADFTransferEnabler adfTransferEnabler)
            : base(
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

        protected override Task<IImportJob> CreateImportJobAsync(INonDocumentObjectLinkingConfiguration configuration, IBatch batch, CancellationToken token)
        {
            return ImportJobFactory.CreateRdoLinkingJobAsync(configuration, batch, token);
        }

        protected override void UpdateImportSettings(INonDocumentObjectLinkingConfiguration configuration)
        {
            configuration.IdentityFieldId = GetDestinationIdentityFieldId();
        }

        protected override void ChildReportBatchMetrics(int batchId, BatchProcessResult batchProcessResult, TimeSpan batchTime, TimeSpan importApiTimer)
        {
            SyncMetrics.Send(new NonDocumentObjectLinkingBatchEndMetric
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
            return Task.FromResult(TaggingExecutionResult.Success());
        }

        protected override Guid GetExportRunId(INonDocumentObjectLinkingConfiguration configuration)
        {
            return configuration.ObjectLinkingSnapshotId
                   ?? throw new NullReferenceException($"{nameof(INonDocumentObjectLinkingConfiguration.ObjectLinkingSnapshotId)} cannot be null at this stage");
        }
    }
}
