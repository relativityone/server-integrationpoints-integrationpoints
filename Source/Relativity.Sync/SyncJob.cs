﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Banzai;

namespace Relativity.Sync
{
	internal sealed class SyncJob : ISyncJob
	{
		private readonly INode<SyncExecutionContext> _pipeline;
		private readonly ISyncExecutionContextFactory _executionContextFactory;
		private readonly CorrelationId _correlationId;
		private readonly ISyncLog _logger;

		public SyncJob(INode<SyncExecutionContext> pipeline, ISyncExecutionContextFactory executionContextFactory, CorrelationId correlationId, ISyncLog logger)
		{
			_pipeline = pipeline;
			_executionContextFactory = executionContextFactory;
			_correlationId = correlationId;
			_logger = logger;
		}

		public async Task ExecuteAsync(CancellationToken token)
		{
			await ExecuteAsync(new EmptyProgress(), token).ConfigureAwait(false);
		}

		public async Task ExecuteAsync(IProgress<SyncProgress> progress, CancellationToken token)
		{
			NodeResult executionResult;
			try
			{
				IExecutionContext<SyncExecutionContext> executionContext = _executionContextFactory.Create(progress, token);
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

			if (executionResult.Status != NodeResultStatus.Succeeded)
			{
				throw new SyncException("Sync job failed. See inner exceptions for more details.", new AggregateException(executionResult.GetFailExceptions()), _correlationId.Value);
			}
		}

		public async Task RetryAsync(CancellationToken token)
		{
			await RetryAsync(new EmptyProgress(), token).ConfigureAwait(false);
		}

		public Task RetryAsync(IProgress<SyncProgress> progress, CancellationToken token)
		{
			throw new NotImplementedException();
		}
	}
}