﻿using System;
using System.Threading.Tasks;
using Banzai;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal abstract class SyncNode<T> : Node<SyncExecutionContext> where T : IConfiguration
	{
		private readonly ICommand<T> _command;
		private readonly ISyncLog _logger;

		protected SyncNode(ICommand<T> command, ISyncLog logger)
		{
			_command = command;
			_logger = logger;
		}

		protected SyncNode(ExecutionOptions localOptions, ICommand<T> command, ISyncLog logger) : base(localOptions)
		{
			_command = command;
			_logger = logger;
		}

		public override async Task<bool> ShouldExecuteAsync(IExecutionContext<SyncExecutionContext> context)
		{
			return await _command.CanExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false);
		}

		protected override async Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<SyncExecutionContext> context)
		{
			try
			{
				await _command.ExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception occurred when executing step.");
				throw;
			}

			return NodeResultStatus.Succeeded;
		}

		protected override void OnBeforeExecute(IExecutionContext<SyncExecutionContext> context)
		{
			SyncProgress progress = new SyncProgress(Id);
			context.Subject.Progress.Report(progress);
		}
	}
}