using System;
using Banzai;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class SyncRootNode : PipelineNodeBase<SyncExecutionContext>
	{
		private readonly ICommand<INotificationConfiguration> _command;
		private readonly ISyncLog _logger;

		public SyncRootNode(ICommand<INotificationConfiguration> command, ISyncLog logger)
		{
			_command = command;
			_logger = logger;
			Id = "SyncRoot";
		}

		protected override void OnAfterExecute(IExecutionContext<SyncExecutionContext> context)
		{
			SyncJobState jobState = new SyncJobState("Sending notifications");
			context.Subject.Progress.Report(jobState);

			if (_command.CanExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				ExecutionResult result = _command.ExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

				// We really shouldn't be throwing if we can't return a proper result, so we'll just log the error here.
				if (result.Status == ExecutionStatus.Failed)
				{
					// result.Exception may be null, but this should not cause any issues.
					_logger.LogError(result.Exception, "Failed to send notifications: {message}", result.Message);
				}
			}
		}
	}
}