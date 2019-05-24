using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportJob : IImportJob
	{
		private bool _itemLevelErrorExists;
		private Exception _importApiException;

		private const string _IDENTIFIER_COLUMN = "Identifier";
		private const string _MESSAGE_COLUMN = "Message";

		private readonly int _jobHistoryArtifactId;
		private readonly int _sourceWorkspaceArtifactId;

		private readonly ISyncImportBulkArtifactJob _syncImportBulkArtifactJob;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly ISemaphoreSlim _semaphoreSlim;
		private readonly ISyncLog _logger;

		public ImportJob(ISyncImportBulkArtifactJob syncImportBulkArtifactJob, ISemaphoreSlim semaphoreSlim, IJobHistoryErrorRepository jobHistoryErrorRepository,
			int sourceWorkspaceArtifactId, int jobHistoryArtifactId, ISyncLog syncLog)
		{
			_itemLevelErrorExists = false;
			_importApiException = null;

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
			if (_importApiException == null)
			{
				_syncImportBulkArtifactJob.ItemStatusMonitor.MarkReadSoFarAsSuccessful();
				_logger.LogInformation("Batch completed.");
				_semaphoreSlim.Release();
			}
		}

		private void HandleFatalException(JobReport jobReport)
		{
			_logger.LogError(jobReport.FatalException, jobReport.FatalException?.Message);
			_importApiException = jobReport.FatalException;

			_syncImportBulkArtifactJob.ItemStatusMonitor.MarkReadSoFarAsFailed();
			var jobError = new CreateJobHistoryErrorDto(_jobHistoryArtifactId, ErrorType.Job)
			{
				ErrorMessage = jobReport.FatalException?.Message,
				StackTrace = jobReport.FatalException?.StackTrace
			};
			CreateJobHistoryError(jobError);

			_semaphoreSlim.Release();
		}

		private void HandleItemLevelError(IDictionary row)
		{
			_itemLevelErrorExists = true;

			string errorMessage = $"IAPI {GetValueOrNull(row, _MESSAGE_COLUMN)}";
			string sourceUniqueId = GetValueOrNull(row, _IDENTIFIER_COLUMN);

			_logger.LogError("Item level error occurred. Source: {sourceUniqueId} Message: {errorMessage}", sourceUniqueId, errorMessage);

			_syncImportBulkArtifactJob.ItemStatusMonitor.MarkItemAsFailed(sourceUniqueId);
			var itemError = new CreateJobHistoryErrorDto(_jobHistoryArtifactId, ErrorType.Item)
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

		public async Task<ExecutionResult> RunAsync(CancellationToken token)
		{
			ExecutionResult executionResult = ExecutionResult.Success();
			if (token.IsCancellationRequested)
			{
				executionResult = ExecutionResult.Canceled();
				return executionResult;
			}

			try
			{
				await Task.Run(() => _syncImportBulkArtifactJob.Execute(), token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to start executing import job.");
				throw;
			}

			// we don't want to cancel waiting for the import job to finish
			// we instead periodically check the token in the IAPI events
			// and release the semaphore as needed
			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

			if (_importApiException != null)
			{
				const string fatalExceptionMessage = "Fatal exception occurred in Import API.";
				_logger.LogError(_importApiException, fatalExceptionMessage);

				var syncException = new SyncException(fatalExceptionMessage, _importApiException);
				executionResult = ExecutionResult.Failure(fatalExceptionMessage, syncException);
			}
			else if (_itemLevelErrorExists)
			{
				const string completedWithErrors = "Import completed with item level errors.";
				executionResult = new ExecutionResult(ExecutionStatus.CompletedWithErrors, completedWithErrors, null);
			}
			return executionResult;
		}

		public async Task<IEnumerable<int>> GetPushedDocumentArtifactIds()
		{
			await Task.Yield();
			return _syncImportBulkArtifactJob.ItemStatusMonitor.GetSuccessfulItemArtifactIds();
		}

		public void Dispose()
		{
			_semaphoreSlim?.Dispose();
		}
	}
}