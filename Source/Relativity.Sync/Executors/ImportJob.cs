using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportJob : IImportJob
	{
		private bool _jobCompletedSuccessfully;
		private Exception _importApiException;

		private const string _IDENTIFIER_COLUMN = "Identifier";
		private const string _MESSAGE_COLUMN = "Message";

		private readonly IImportBulkArtifactJob _importBulkArtifactJob;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly int _jobHistoryArtifactId;
		private readonly int _sourceWorkspaceArtifactId;
		private readonly ISemaphoreSlim _semaphoreSlim;
		private readonly ISyncLog _logger;

		public ImportJob(IImportBulkArtifactJob importBulkArtifactJob, ISemaphoreSlim semaphoreSlim, IJobHistoryErrorRepository jobHistoryErrorRepository,
			int sourceWorkspaceArtifactId, int jobHistoryArtifactId, ISyncLog syncLog)
		{
			_importBulkArtifactJob = importBulkArtifactJob;
			_semaphoreSlim = semaphoreSlim;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_sourceWorkspaceArtifactId = sourceWorkspaceArtifactId;
			_jobHistoryArtifactId = jobHistoryArtifactId;
			_logger = syncLog;

			_importBulkArtifactJob.OnComplete += HandleComplete;
			_importBulkArtifactJob.OnFatalException += HandleFatalException;
			_importBulkArtifactJob.OnError += HandleItemLevelError;
		}

		private void HandleComplete(JobReport jobReport)
		{
			_logger.LogInformation("Batch completed.");
			_jobCompletedSuccessfully = true;
			_semaphoreSlim.Release();
		}

		private void HandleFatalException(JobReport jobReport)
		{
			const string errorMessage = "Fatal exception occurred in ImportAPI during import job";
			_logger.LogError(jobReport.FatalException, errorMessage);
			_jobCompletedSuccessfully = false;
			_importApiException = jobReport.FatalException;

			CreateJobHistoryErrorDto jobError = new CreateJobHistoryErrorDto(_jobHistoryArtifactId, ErrorType.Job)
			{
				ErrorMessage = errorMessage,
				StackTrace = jobReport.FatalException.StackTrace
			};
			CreateJobHistoryError(jobError);

			_semaphoreSlim.Release();
		}

		private void HandleItemLevelError(IDictionary row)
		{
			string errorMessage = GetValueOrNull(row, _MESSAGE_COLUMN);
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

		private string GetValueOrNull(IDictionary row, string key)
		{
			return row.Contains(key) ? row[key].ToString() : null;
		}

		public async Task RunAsync(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			await Task.Run(_importBulkArtifactJob.Execute, token).ConfigureAwait(false);
			await _semaphoreSlim.WaitAsync(token).ConfigureAwait(false);

			if (!_jobCompletedSuccessfully)
			{
				throw new SyncException("Import job did not completed successfully.", _importApiException);
			}
		}
	}
}