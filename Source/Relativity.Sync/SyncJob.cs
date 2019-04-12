using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banzai;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync
{
	internal sealed class SyncJob : ISyncJob
	{
		private readonly INode<SyncExecutionContext> _pipeline;
		private readonly ISyncExecutionContextFactory _executionContextFactory;
		private readonly CorrelationId _correlationId;
		private readonly IProgress<SyncJobState> _syncProgress;
		private readonly ISyncLog _logger;

		public SyncJob(INode<SyncExecutionContext> pipeline, ISyncExecutionContextFactory executionContextFactory, CorrelationId correlationId, IProgress<SyncJobState> syncProgress, ISyncLog logger)
		{
			_pipeline = pipeline;
			_executionContextFactory = executionContextFactory;
			_correlationId = correlationId;
			_syncProgress = syncProgress;
			_logger = logger;
		}

		public async Task ExecuteAsync(CancellationToken token)
		{
			await ExecuteAsync(token, _syncProgress).ConfigureAwait(false);
		}

		public async Task ExecuteAsync(IProgress<SyncJobState> progress, CancellationToken token)
		{
			IProgress<SyncJobState> safeProgress = new SafeProgressWrapper<SyncJobState>(progress, _logger);
			await ExecuteAsync(token, _syncProgress, safeProgress).ConfigureAwait(false);
		}

		private async Task ExecuteAsync(CancellationToken token, params IProgress<SyncJobState>[] progressReporters)
		{
			NodeResult executionResult;
			try
			{
				IProgress<SyncJobState> combinedProgress = progressReporters.Combine();
				IExecutionContext<SyncExecutionContext> executionContext = _executionContextFactory.Create(combinedProgress, token);
				executionResult = await _pipeline.ExecuteAsync(executionContext).ConfigureAwait(false);
			}
			catch (OperationCanceledException e)
			{
				_logger.LogWarning(e, "Operation cancelled.");
				throw;
			}
			catch (SyncException e)
			{
				_logger.LogError(e, "SyncException has been thrown during job execution.");
				throw;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error occured during Sync job execution.");
				throw new SyncException("Error occured during Sync job execution. See inner exception for more details.", e, _correlationId.Value);
			}

			if (executionResult.Status != NodeResultStatus.Succeeded && executionResult.Status != NodeResultStatus.SucceededWithErrors)
			{
				SyncExecutionContext subject = (SyncExecutionContext)executionResult.Subject;
				IList<Exception> failingExceptions = subject.Results
					.Where(r => r.Exception != null)
					.Select(r => r.Exception)
					.ToList();

				ValidationException validationException = failingExceptions.OfType<ValidationException>().FirstOrDefault();
				if (validationException != null)
				{
					throw new ValidationException(validationException.Message, new AggregateException(failingExceptions), validationException.ValidationResult);
				}

				throw new SyncException("Sync job failed. See inner exceptions for more details.", new AggregateException(failingExceptions), _correlationId.Value);
			}
		}

		public async Task RetryAsync(CancellationToken token)
		{
			await RetryAsync(token, _syncProgress).ConfigureAwait(false);
		}

		public async Task RetryAsync(IProgress<SyncJobState> progress, CancellationToken token)
		{
			IProgress<SyncJobState> safeProgress = new SafeProgressWrapper<SyncJobState>(progress, _logger);
			await RetryAsync(token, _syncProgress, safeProgress).ConfigureAwait(false);
		}

		private Task RetryAsync(CancellationToken token, params IProgress<SyncJobState>[] progressReporters)
		{
			throw new NotImplementedException();
		}
	}
}