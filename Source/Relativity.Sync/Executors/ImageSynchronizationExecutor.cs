using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class ImageSynchronizationExecutor : SynchronizationExecutorBase<IImageSynchronizationConfiguration>
    {
        private readonly IDocumentTagger _documentTagger;

		public ImageSynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository,
			IJobProgressHandlerFactory jobProgressHandlerFactory, 
			IFieldManager fieldManager, IFieldMappings fieldMappings, IJobStatisticsContainer jobStatisticsContainer,
			IJobCleanupConfiguration jobCleanupConfiguration,
			IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
			Func<IStopwatch> stopwatchFactory, ISyncMetrics syncMetrics, IDocumentTagger documentTagger, IAPILog logger,
            IADLSUploader uploader, IUserContextConfiguration userContextConfiguration, IADFTransferEnabler adfTransferEnabler)
			: base(importJobFactory, BatchRecordType.Images, batchRepository, jobProgressHandlerFactory, fieldManager,
			fieldMappings, jobStatisticsContainer, jobCleanupConfiguration, automatedWorkflowTriggerConfiguration, 
            stopwatchFactory, syncMetrics, userContextConfiguration, uploader, adfTransferEnabler, logger)
		{
			_documentTagger = documentTagger;
		}

        protected override Task<IImportJob> CreateImportJobAsync(IImageSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
        {
            return ImportJobFactory.CreateImageImportJobAsync(configuration, batch, token);
        }

        protected override void UpdateImportSettings(IImageSynchronizationConfiguration configuration)
        {
            configuration.IdentityFieldId = GetDestinationIdentityFieldId();

            IList<FieldInfoDto> specialFields = FieldManager.GetImageSpecialFields().ToList();
            configuration.ImageFilePathSourceFieldName = GetSpecialFieldColumnName(specialFields, SpecialFieldType.ImageFileLocation);
            configuration.FileNameColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.ImageFileName);
            configuration.IdentifierColumn = GetSpecialFieldColumnName(specialFields, SpecialFieldType.ImageIdentifier);
        }

        protected override void ChildReportBatchMetrics(int batchId, BatchProcessResult batchProcessResult, TimeSpan batchTime, TimeSpan importApiTimer)
        {
            Telemetry.SyncMetrics.Send(new ImageBatchEndMetric()
            {
                TotalRecordsRequested = batchProcessResult.TotalRecordsRequested,
                TotalRecordsTransferred = batchProcessResult.TotalRecordsTransferred,
                TotalRecordsFailed = batchProcessResult.TotalRecordsFailed,
                TotalRecordsTagged = batchProcessResult.TotalRecordsTagged,
                BytesNativesTransferred = batchProcessResult.FilesBytesTransferred,
                BytesMetadataTransferred = batchProcessResult.MetadataBytesTransferred,
                BytesTransferred = batchProcessResult.BytesTransferred,
                BatchImportAPITime = importApiTimer.TotalMilliseconds,
                BatchTotalTime = batchTime.TotalMilliseconds,
            });
        }

        protected override Task<TaggingExecutionResult> TagObjectsAsync(IImportJob importJob, ISynchronizationConfiguration configuration,
            CompositeCancellationToken token)
        {
            return _documentTagger.TagObjectsAsync(importJob, configuration, token);
        }
    }
}
