using System;
using System.Threading;
using System.Threading.Tasks;
using Banzai;

namespace Relativity.Sync
{
	internal sealed class SyncJob : ISyncJob
	{
		private readonly INode<SyncExecutionContext> _pipeline;
		private readonly ISyncExecutionContextFactory _executionContextFactory;

		public SyncJob(INode<SyncExecutionContext> pipeline, ISyncExecutionContextFactory executionContextFactory)
		{
			_pipeline = pipeline;
			_executionContextFactory = executionContextFactory;
		}

		public async Task ExecuteAsync(CancellationToken token)
		{
			await ExecuteAsync(new EmptyProgress(), token).ConfigureAwait(false);
		}

		public async Task ExecuteAsync(IProgress<SyncProgress> progress, CancellationToken token)
		{
			IExecutionContext<SyncExecutionContext> executionContext = _executionContextFactory.Create(progress, token);
			NodeResult executionResult = await _pipeline.ExecuteAsync(executionContext).ConfigureAwait(false);
			if (executionResult.Status != NodeResultStatus.Succeeded)
			{
				throw new SyncException("Sync job failed", executionResult.Exception);
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