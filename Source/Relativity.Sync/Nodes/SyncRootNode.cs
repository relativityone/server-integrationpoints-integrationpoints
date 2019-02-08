using Banzai;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class SyncRootNode : PipelineNodeBase<SyncExecutionContext>
	{
		private readonly ICommand<INotificationConfiguration> _command;

		public SyncRootNode(ICommand<INotificationConfiguration> command)
		{
			_command = command;
			Id = "SyncRoot";
		}

		protected override void OnAfterExecute(IExecutionContext<SyncExecutionContext> context)
		{
			SyncProgress progress = new SyncProgress("Sending notifications");
			context.Subject.Progress.Report(progress);

			if (_command.CanExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				_command.ExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
	}
}