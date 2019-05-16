using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Executors
{
	internal sealed class SynchronizationExecutor : IExecutor<ISynchronizationConfiguration>
	{
		private readonly IImportJobFactory _importJobFactory;
		private readonly IBatchRepository _batchRepository;
		private readonly ISyncMetrics _syncMetrics;
		private readonly IDateTime _dateTime;
		private readonly ISyncLog _logger;

		public SynchronizationExecutor(IImportJobFactory importJobFactory, IBatchRepository batchRepository, ISyncMetrics syncMetrics, IDateTime dateTime, ISyncLog logger)
		{
			_importJobFactory = importJobFactory;
			_batchRepository = batchRepository;
			_syncMetrics = syncMetrics;
			_dateTime = dateTime;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			ExecutionResult result = ExecutionResult.Success();
			DateTime startTime = _dateTime.Now;

			try
			{
				_logger.LogVerbose("Gathering batches to execute.");

				List<int> batchesIds = (await _batchRepository.GetAllNewBatchesIdsAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false)).ToList();

				foreach (int batchId in batchesIds)
				{
					if (token.IsCancellationRequested)
					{
						_logger.LogInformation("Import job has been canceled.");
						result = ExecutionResult.Canceled();
						break;
					}

					IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);
					_logger.LogVerbose("Processing batch ID: {batchId}", batchId);
					using (IImportJob importJob = _importJobFactory.CreateImportJob(configuration, batch))
					{
						await importJob.RunAsync(token).ConfigureAwait(false);
					}
					_logger.LogInformation("Batch ID: {batchId} processed successfully.", batchId);
				}
			}
			catch (SyncException ex)
			{
				const string message = "Fatal exception occurred while executing import job.";
				_logger.LogError(ex, message);
				result = ExecutionResult.Failure(message, ex);
			}
			catch (Exception ex)
			{
				const string message = "Unexpected exception occurred while executing import job.";
				_logger.LogError(ex, message);
				result = ExecutionResult.Failure(message, ex);
			}
			finally
			{
				// TODO metrics
				DateTime endTime = _dateTime.Now;
				TimeSpan jobDuration = endTime - startTime;
				_syncMetrics.CountOperation("ImportJobStatus", result.Status);
				_syncMetrics.TimedOperation("ImportJob", jobDuration, result.Status);
				_syncMetrics.GaugeOperation("ImportJobStart", result.Status, startTime.Ticks, "Ticks", null);
				_syncMetrics.GaugeOperation("ImportJobEnd", result.Status, endTime.Ticks, "Ticks", null);
			}

			return result;
		}
	}
}