using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportJob : IImportJob
	{
		private bool _importApiFatalExceptionOccurred;
		private bool _itemLevelErrorExists;
		private bool _canReleaseSemaphore;
		private Exception _importApiException;
		private JobReport _importApiJobReport;

		private const int _ITEMLEVEL_ERRORS_MASS_CREATE_SIZE = 10000;
		private const string _IDENTIFIER_COLUMN = "Identifier";
		private const string _MESSAGE_COLUMN = "Message";

		private readonly object _lockObject;
		private readonly int _jobHistoryArtifactId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly ConcurrentQueue<CreateJobHistoryErrorDto> _itemLevelErrors;

		private readonly ISyncImportBulkArtifactJob _syncImportBulkArtifactJob;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly ISemaphoreSlim _semaphoreSlim;
		private readonly ISyncLog _logger;

		public ImportJob(ISyncImportBulkArtifactJob syncImportBulkArtifactJob, ISemaphoreSlim semaphoreSlim, IJobHistoryErrorRepository jobHistoryErrorRepository,
			int sourceWorkspaceArtifactId, int jobHistoryArtifactId, ISyncLog syncLog)
		{
			_lockObject = new object();
			_importApiFatalExceptionOccurred = false;
			_itemLevelErrorExists = false;
			_itemLevelErrors = new ConcurrentQueue<CreateJobHistoryErrorDto>();
			_canReleaseSemaphore = true;
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
			_importApiJobReport = jobReport;

			// IAPI may throw OnFatalException event before OnComplete, so we need to check that first.
			if (!_importApiFatalExceptionOccurred)
			{
				_syncImportBulkArtifactJob.ItemStatusMonitor.MarkReadSoFarAsSuccessful();
				_logger.LogInformation("Batch completed.");
			}

			MassCreateItemLevelErrorsIfAny();
			ReleaseSemaphoreIfPossible();
		}

		private void HandleFatalException(JobReport jobReport)
		{
			_importApiJobReport = jobReport;

			_logger.LogError(jobReport.FatalException, jobReport.FatalException?.Message);
			_importApiFatalExceptionOccurred = true;
			_importApiException = jobReport.FatalException;

			_syncImportBulkArtifactJob.ItemStatusMonitor.MarkReadSoFarAsFailed();
			var jobError = new CreateJobHistoryErrorDto(ErrorType.Job)
			{
				ErrorMessage = jobReport.FatalException?.Message,
				StackTrace = jobReport.FatalException?.StackTrace
			};

			CreateJobHistoryError(jobError);
			ReleaseSemaphoreIfPossible();
		}

		private void ReleaseSemaphoreIfPossible()
		{
			lock (_lockObject)
			{
				if (_canReleaseSemaphore)
				{
					_semaphoreSlim.Release();
					_canReleaseSemaphore = false;
				}
			}
		}

		private void HandleItemLevelError(IDictionary row)
		{
			_itemLevelErrorExists = true;

			string errorMessage = $"IAPI {GetValueOrNull(row, _MESSAGE_COLUMN)}";
			string sourceUniqueId = GetValueOrNull(row, _IDENTIFIER_COLUMN);

			_logger.LogError("Item level error occurred. Source: {sourceUniqueId} Message: {errorMessage}", sourceUniqueId, errorMessage);

			_syncImportBulkArtifactJob.ItemStatusMonitor.MarkItemAsFailed(sourceUniqueId);
			AddItemLevelError(sourceUniqueId, errorMessage);
		}

		private void AddItemLevelError(string sourceUniqueId, string errorMessage)
		{
			var itemError = new CreateJobHistoryErrorDto(ErrorType.Item)
			{
				ErrorMessage = errorMessage,
				SourceUniqueId = sourceUniqueId
			};

			_itemLevelErrors.Enqueue(itemError);

			if (_itemLevelErrors.Count >= _ITEMLEVEL_ERRORS_MASS_CREATE_SIZE)
			{
				MassCreateItemLevelErrorsIfAny();
			}
		}

		private void MassCreateItemLevelErrorsIfAny()
		{
			List<CreateJobHistoryErrorDto> itemLevelErrors = new List<CreateJobHistoryErrorDto>(_itemLevelErrors.Count);
			while (_itemLevelErrors.TryDequeue(out CreateJobHistoryErrorDto dto))
			{
				itemLevelErrors.Add(dto);
			}

			if (itemLevelErrors.Any())
			{
				_jobHistoryErrorRepository.MassCreateAsync(_sourceWorkspaceArtifactId, _jobHistoryArtifactId, itemLevelErrors).GetAwaiter().GetResult();
			}
		}

		private void CreateJobHistoryError(CreateJobHistoryErrorDto jobError)
		{
			_jobHistoryErrorRepository.CreateAsync(_sourceWorkspaceArtifactId, _jobHistoryArtifactId, jobError).ConfigureAwait(false);
		}

		private static string GetValueOrNull(IDictionary row, string key)
		{
			return row.Contains(key) ? row[key].ToString() : null;
		}

		public async Task<ImportJobResult> RunAsync(CancellationToken token)
		{
			ExecutionResult executionResult = ExecutionResult.Success();
			long jobSizeInBytes = GetJobSize();
			if (token.IsCancellationRequested)
			{
				executionResult = ExecutionResult.Canceled();
				return new ImportJobResult(executionResult, jobSizeInBytes);
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

			// Since the import job doesn't support cancellation, we also don't want to cancel waiting for the job to finish.
			// If it's started, we have to wait and release the semaphore as needed in the IAPI events.
			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

			if (_importApiFatalExceptionOccurred)
			{
				const string fatalExceptionMessage = "Fatal exception occurred in Import API.";
				_logger.LogError(_importApiException, fatalExceptionMessage);

				var syncException = new ImportFailedException(fatalExceptionMessage, _importApiException);
				executionResult = ExecutionResult.Failure(fatalExceptionMessage, syncException);
			}
			else if (_itemLevelErrorExists)
			{
				const string completedWithErrors = "Import completed with item level errors.";
				executionResult = new ExecutionResult(ExecutionStatus.CompletedWithErrors, completedWithErrors, null);
			}

			jobSizeInBytes = GetJobSize();
			return new ImportJobResult(executionResult, jobSizeInBytes);
		}

		private long GetJobSize()
		{
			long jobSize = 0;
			if (_importApiJobReport != null)
			{
				jobSize = _importApiJobReport.FileBytes + _importApiJobReport.MetadataBytes;
			}
			return jobSize;
		}

		public Task<IEnumerable<int>> GetPushedDocumentArtifactIds()
		{
			return Task.FromResult(_syncImportBulkArtifactJob.ItemStatusMonitor.GetSuccessfulItemArtifactIds());
		}

		public Task<IEnumerable<string>> GetPushedDocumentIdentifiers()
		{
			return Task.FromResult(_syncImportBulkArtifactJob.ItemStatusMonitor.GetSuccessfulItemIdentifiers());
		}

		public ISyncImportBulkArtifactJob SyncImportBulkArtifactJob => _syncImportBulkArtifactJob;

		public void Dispose()
		{
			_semaphoreSlim?.Dispose();
		}
	}
}