using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportJob : IImportJob
	{
		private bool _jobCompletedSuccessfully;
		private Exception _importApiException;

		private readonly IImportBulkArtifactJob _importBulkArtifactJob;
		private readonly ISemaphoreSlim _semaphoreSlim;
		private readonly ISyncLog _logger;

		public ImportJob(IBatch batch, IBatchProgressHandlerFactory batchProgressHandlerFactory, IImportBulkArtifactJob importBulkArtifactJob, ISemaphoreSlim semaphoreSlim, ISyncLog syncLog)
		{
			_importBulkArtifactJob = importBulkArtifactJob;
			_semaphoreSlim = semaphoreSlim;
			_logger = syncLog;

			_importBulkArtifactJob.OnComplete += HandleComplete;
			_importBulkArtifactJob.OnFatalException += HandleFatalException;

			batchProgressHandlerFactory.CreateBatchProgressHandler(batch, _importBulkArtifactJob);
		}

		private void HandleFatalException(JobReport jobReport)
		{
			_logger.LogError(jobReport.FatalException, "Fatal exception occurred in ImportAPI during import job");
			_jobCompletedSuccessfully = false;
			_importApiException = jobReport.FatalException;
			_semaphoreSlim.Release();
		}

		private void HandleComplete(JobReport jobReport)
		{
			_logger.LogInformation("Import completed.");
			_jobCompletedSuccessfully = true;
			_semaphoreSlim.Release();
		}

		public async Task RunAsync(CancellationToken token)
		{
			// TODO how to cancel IAPI job?
			await Task.Run(_importBulkArtifactJob.Execute, token).ConfigureAwait(false);
			await _semaphoreSlim.WaitAsync(token).ConfigureAwait(false);

			if (!_jobCompletedSuccessfully)
			{
				throw new SyncException("Import job did not completed successfully.", _importApiException);
			}
		}
	}
}