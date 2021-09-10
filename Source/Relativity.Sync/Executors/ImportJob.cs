using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportJob : IImportJob
	{
		private bool _importApiFatalExceptionOccurred;
		private bool _itemLevelErrorExists;
		private bool _canReleaseSemaphore;
		private Exception _importApiException;
		private ImportApiJobStatistics _importApiJobStatistics;

		private const int _ITEMLEVEL_ERRORS_MASS_CREATE_SIZE = 10000;

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
			_syncImportBulkArtifactJob.OnItemLevelError += HandleItemLevelError;
		}

		private void HandleComplete(ImportApiJobStatistics jobStatistics)
		{
			_importApiJobStatistics = jobStatistics;

			// IAPI may throw OnFatalException event before OnComplete, so we need to check that first.
			if (!_importApiFatalExceptionOccurred)
			{
				_syncImportBulkArtifactJob.ItemStatusMonitor.MarkReadSoFarAsSuccessful();
				_logger.LogInformation("Batch completed.");
			}

			MassCreateItemLevelErrorsIfAny();
			ReleaseSemaphoreIfPossible();
		}

		private void HandleFatalException(ImportApiJobStatistics jobStatistics)
		{
			_importApiJobStatistics = jobStatistics;

			_logger.LogError(jobStatistics.Exception, jobStatistics.Exception?.Message);
			_importApiFatalExceptionOccurred = true;
			_importApiException = jobStatistics.Exception;

			_syncImportBulkArtifactJob.ItemStatusMonitor.MarkReadSoFarAsFailed();
			var jobError = new CreateJobHistoryErrorDto(ErrorType.Job)
			{
				ErrorMessage = jobStatistics.Exception?.Message,
				StackTrace = jobStatistics.Exception?.StackTrace
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

		private void HandleItemLevelError(ItemLevelError itemLevelError)
		{
			_itemLevelErrorExists = true;
			
			_logger.LogWarning("Item level error occurred. ArtifactId: {itemArtifactId} Message: {errorMessage}",
				_syncImportBulkArtifactJob.ItemStatusMonitor.GetArtifactId(itemLevelError.Identifier), itemLevelError.Message);

			_syncImportBulkArtifactJob.ItemStatusMonitor.MarkItemAsFailed(itemLevelError.Identifier);
			AddItemLevelError(itemLevelError.Identifier, itemLevelError.Message);
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

		public async Task<ImportJobResult> RunAsync(CompositeCancellationToken token)
		{
			ExecutionResult executionResult = ExecutionResult.Success();

			if (token.IsStopRequested)
			{
				executionResult = ExecutionResult.Canceled();
				return new ImportJobResult(executionResult, GetMetadataSize(), GetFilesSize(), GetJobSize());
			}
			if (token.IsDrainStopRequested)
			{
				executionResult = ExecutionResult.Paused();
				return new ImportJobResult(executionResult, GetMetadataSize(), GetFilesSize(), GetJobSize());
			}

			try
			{
				await Task.Run(() => _syncImportBulkArtifactJob.Execute(), token.AnyReasonCancellationToken).ConfigureAwait(false);
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
			else if (token.IsDrainStopRequested)
			{
				_logger.LogInformation("IAPI job has finished due to drain-stop. Returning Paused execution result.");
				executionResult = ExecutionResult.Paused();
			}
			else if (_itemLevelErrorExists)
			{
				_logger.LogInformation("IAPI job has finished with item level errors.");
				const string completedWithErrors = "Import completed with item level errors.";
				executionResult = new ExecutionResult(ExecutionStatus.CompletedWithErrors, completedWithErrors, null);
			}

			return new ImportJobResult(executionResult, GetMetadataSize(), GetFilesSize(), GetJobSize());
		}

		private long GetMetadataSize()
		{
			long metadataSize = 0;
			if (_importApiJobStatistics != null)
			{
				metadataSize = _importApiJobStatistics.MetadataBytes;
			}
			return metadataSize;
		}

		private long GetFilesSize()
		{
			long metadataSize = 0;
			if (_importApiJobStatistics != null)
			{
				metadataSize = _importApiJobStatistics.FileBytes;
			}
			return metadataSize;
		}

		private long GetJobSize()
		{
			long jobSize = 0;
			if (_importApiJobStatistics != null)
			{
				jobSize = _importApiJobStatistics.FileBytes + _importApiJobStatistics.MetadataBytes;
			}
			return jobSize;
		}

		public Task<IEnumerable<int>> GetPushedDocumentArtifactIdsAsync()
		{
			return Task.FromResult(_syncImportBulkArtifactJob.ItemStatusMonitor.GetSuccessfulItemArtifactIds());
		}

		public Task<IEnumerable<string>> GetPushedDocumentIdentifiersAsync()
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