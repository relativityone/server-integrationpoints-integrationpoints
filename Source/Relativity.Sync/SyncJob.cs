﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banzai;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync
{
	internal sealed class SyncJob : ISyncJob
	{
		private readonly INode<SyncExecutionContext> _pipeline;
		private readonly ISyncExecutionContextFactory _executionContextFactory;
		private readonly SyncJobParameters _syncJobParameters;
		private readonly IProgress<SyncJobState> _syncProgress;
		private readonly ISyncLog _logger;

		public SyncJob(INode<SyncExecutionContext> pipeline, ISyncExecutionContextFactory executionContextFactory, SyncJobParameters syncJobParameters, IProgress<SyncJobState> syncProgress, ISyncLog logger)
		{
			_pipeline = pipeline;
			_executionContextFactory = executionContextFactory;
			_syncJobParameters = syncJobParameters;
			_syncProgress = syncProgress;
			_logger = logger;
		}

		public Task ExecuteAsync(CompositeCancellationToken token)
		{
			return ExecuteAsync(token, _syncProgress);
		}

		public Task ExecuteAsync(IProgress<SyncJobState> progress, CompositeCancellationToken token)
		{
			IProgress<SyncJobState> safeProgress = new SafeProgressWrapper<SyncJobState>(progress, _logger);
			return ExecuteAsync(token, _syncProgress, safeProgress);
		}

		private async Task ExecuteAsync(CompositeCancellationToken token, params IProgress<SyncJobState>[] progressReporters)
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
				throw new SyncException("Error occured during Sync job execution. See inner exception for more details.", e, _syncJobParameters.WorkflowId);
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

				throw new SyncException("Sync job failed. See inner exceptions for more details.", new AggregateException(failingExceptions), _syncJobParameters.WorkflowId);
			}
		}
	}
}