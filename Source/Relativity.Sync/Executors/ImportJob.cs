using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportJob : IImportJob
	{
		private bool _importApiFatalExceptionOccurred = false;
		private Exception _importApiException = null;

		private const string _IDENTIFIER_COLUMN = "Identifier";
		private const string _MESSAGE_COLUMN = "Message";

		private readonly ISyncImportBulkArtifactJob _syncImportBulkArtifactJob;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly int _jobHistoryArtifactId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly ISemaphoreSlim _semaphoreSlim;
		private readonly ISyncLog _logger;

		public ImportJob(ISyncImportBulkArtifactJob syncImportBulkArtifactJob, ISemaphoreSlim semaphoreSlim, IJobHistoryErrorRepository jobHistoryErrorRepository,
			int sourceWorkspaceArtifactId, int jobHistoryArtifactId, ISyncLog syncLog)
		{
			_syncImportBulkArtifactJob = syncImportBulkArtifactJob;
			_semaphoreSlim = semaphoreSlim;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_jobHistoryArtifactId = jobHistoryArtifactId;
			_logger = syncLog;

			_syncImportBulkArtifactJob.OnComplete += HandleComplete;
			_syncImportBulkArtifactJob.OnFatalException += HandleFatalException;
			_syncImportBulkArtifactJob.OnError += HandleItemLevelError;
		}

		private void HandleComplete(JobReport jobReport)
		{
			// IAPI always fires OnComplete event - even when fatal exception has occurred before, so we need to check that.
			if (!_importApiFatalExceptionOccurred)
			{
				_logger.LogInformation("Batch completed.");
			}

			_semaphoreSlim.Release();
		}

		private void HandleFatalException(JobReport jobReport)
		{
			_logger.LogError(jobReport.FatalException, jobReport.FatalException?.Message);
			_importApiFatalExceptionOccurred = true;
			_importApiException = jobReport.FatalException;

			CreateJobHistoryErrorDto jobError = new CreateJobHistoryErrorDto(_jobHistoryArtifactId, ErrorType.Job)
			{
				ErrorMessage = jobReport.FatalException?.Message,
				StackTrace = jobReport.FatalException?.StackTrace
			};
			CreateJobHistoryError(jobError);
		}

		private void HandleItemLevelError(IDictionary row)
		{
			string errorMessage = $"IAPI {GetValueOrNull(row, _MESSAGE_COLUMN)}";
			string sourceUniqueId = GetValueOrNull(row, _IDENTIFIER_COLUMN);

			_logger.LogError("Item level error occurred. Source: {sourceUniqueId} Message: {errorMessage}", sourceUniqueId, errorMessage);

			CreateJobHistoryErrorDto itemError = new CreateJobHistoryErrorDto(_jobHistoryArtifactId, ErrorType.Item)
			{
				ErrorMessage = errorMessage,
				SourceUniqueId = sourceUniqueId
			};
			CreateJobHistoryError(itemError);
		}

		private void CreateJobHistoryError(CreateJobHistoryErrorDto jobError)
		{
			_jobHistoryErrorRepository.CreateAsync(_sourceWorkspaceArtifactId, jobError).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private static string GetValueOrNull(IDictionary row, string key)
		{
			return row.Contains(key) ? row[key].ToString() : null;
		}

		public async Task RunAsync(CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				return;
			}

			try
			{
				await Task.Run(() => _syncImportBulkArtifactJob.Execute(), token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				const string message = "Failed to start executing import job.";
				_logger.LogError(ex, message);
				throw new ImportFailedException(message, ex);
			}

			// Since the import job doesn't support cancellation, we also don't want to cancel waiting for the job to finish. If it's started, we have to wait.
			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

			if (_importApiFatalExceptionOccurred)
			{
				throw new ImportFailedException("Fatal exception occurred in Import API.", _importApiException);
			}
		}

		public void Dispose()
		{
			_semaphoreSlim?.Dispose();
		}
	}
}