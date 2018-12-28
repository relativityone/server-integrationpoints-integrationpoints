﻿using System;
using System.Threading;
using Banzai;

namespace Relativity.Sync
{
	internal sealed class SyncExecutionContextFactory : ISyncExecutionContextFactory
	{
		private readonly SyncConfiguration _configuration;

		public SyncExecutionContextFactory(SyncConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IExecutionContext<SyncExecutionContext> Create(IProgress<SyncProgress> progress, CancellationToken token)
		{
			SyncExecutionContext subject = new SyncExecutionContext(progress, token);
			ExecutionOptions globalOptions = new ExecutionOptions
			{
				ThrowOnError = false,
				ContinueOnFailure = false,
				DegreeOfParallelism = _configuration.NumberOfStepsRunInParallel
			};
			IExecutionContext<SyncExecutionContext> context = new ExecutionContext<SyncExecutionContext>(subject, globalOptions);

			token.Register(() => context.CancelProcessing = true);

			return context;
		}
	}
}