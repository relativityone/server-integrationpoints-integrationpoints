using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
	internal class NonDocumentSynchronizationExecutor : SynchronizationExecutorBase<INonDocumentSynchronizationConfiguration>
	{
		public NonDocumentSynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository,
			IJobProgressHandlerFactory jobProgressHandlerFactory,
			IFieldManager fieldManager, IFieldMappings fieldMappings, IJobStatisticsContainer jobStatisticsContainer,
			IJobCleanupConfiguration jobCleanupConfiguration,
			IAutomatedWorkflowTriggerConfiguration automatedWorkflowTriggerConfiguration,
			Func<IStopwatch> stopwatchFactory, ISyncMetrics syncMetrics, ISyncLog logger,
			IUserContextConfiguration userContextConfiguration) : base(importJobFactory, BatchRecordType.Documents, batchRepository, jobProgressHandlerFactory, fieldManager,
			fieldMappings, jobStatisticsContainer, jobCleanupConfiguration, automatedWorkflowTriggerConfiguration, stopwatchFactory, syncMetrics, userContextConfiguration, logger)
		{
		}

		protected override Task<IImportJob> CreateImportJobAsync(INonDocumentSynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
		{
			return _importJobFactory.CreateRdoImportJobAsync(configuration, batch, token);
		}

		protected override void UpdateImportSettings(INonDocumentSynchronizationConfiguration configuration)
		{
			throw new NotImplementedException();
		}

		protected override void ChildReportBatchMetrics(int batchId, BatchProcessResult batchProcessResult, TimeSpan batchTime,
			TimeSpan importApiTimer)
		{
			throw new NotImplementedException();
		}

		protected override Task<TaggingExecutionResult> TagDocumentsAsync(IImportJob importJob, ISynchronizationConfiguration configuration,
			CompositeCancellationToken token)
		{
			var dummyResult = TaggingExecutionResult.Success();
			dummyResult.TaggedDocumentsCount = 0;

			return Task.FromResult(dummyResult);
		}
	}
}
