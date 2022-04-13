using Relativity.API;
using System;
using System.Threading.Tasks;
using Banzai;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal abstract class SyncNode<T> : Node<SyncExecutionContext> where T : IConfiguration
	{
		private readonly ICommand<T> _command;
		private readonly IAPILog _logger;

		protected string ParallelGroupName { get; set; } = string.Empty;

		protected SyncNode(ICommand<T> command, IAPILog logger)
		{
			_command = command;
			_logger = logger;
		}

		protected SyncNode(ExecutionOptions localOptions, ICommand<T> command, IAPILog logger) : base(localOptions)
		{
			_command = command;
			_logger = logger;
		}

		public override async Task<bool> ShouldExecuteAsync(IExecutionContext<SyncExecutionContext> context)
		{
			bool canExecute;
			try
			{
				canExecute = await _command.CanExecuteAsync(context.Subject.CompositeCancellationToken.StopCancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Uncaught exception when checking whether step '{stepName}' should execute.", Id);
				context.Subject.Progress.ReportFailure(Id, ParallelGroupName, ex);
				throw;
			}

			if (!canExecute)
			{
				HandleExecutionResult(context, ExecutionResult.Skipped());
			}
			return canExecute;
		}

		protected override async Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<SyncExecutionContext> context)
		{
			ExecutionResult result;
			try
			{
				result = await _command.ExecuteAsync(context.Subject.CompositeCancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Uncaught exception when executing step '{step}'.", Id);
				result = ExecutionResult.Failure($"Uncaught exception when executing step '{Id}'.", ex);
			}

			_logger.LogVerbose($"Step '{{step}}' received {nameof(ExecutionResult)} from command: {{result}}", Id, result);

			context.Subject.Results.Add(result);

			NodeResultStatus status = HandleExecutionResult(context, result);
			return status;
		}

		private NodeResultStatus HandleExecutionResult(IExecutionContext<SyncExecutionContext> context, ExecutionResult result)
		{
			if (result.Status == ExecutionStatus.Failed)
			{
				_logger.LogError(result.Exception, "Error occurred during execution of step '{step}': {message}", Id, result.Message);
				context.Subject.Progress.ReportFailure(Id, ParallelGroupName, result.Exception);

				return NodeResultStatus.Failed;
			}
			else if (result.Status == ExecutionStatus.Canceled)
			{
				_logger.LogInformation("Step '{step}' was canceled during execution.", Id);
				context.Subject.Progress.ReportCanceled(Id, ParallelGroupName);

				return NodeResultStatus.Succeeded;
			}
			else if (result.Status == ExecutionStatus.CompletedWithErrors)
			{
				_logger.LogWarning(result.Exception, "Step '{step}' completed with errors: {message}", Id, result.Message);
				context.Subject.Progress.ReportCompletedWithErrors(Id, ParallelGroupName);

				return NodeResultStatus.SucceededWithErrors;
			}
			else if (result.Status == ExecutionStatus.Skipped)
			{
				_logger.LogInformation("Step '{step}' was skipped.", Id);
				context.Subject.Progress.ReportCompleted(Id, ParallelGroupName);

				return NodeResultStatus.NotRun;
			}
			else
			{
				_logger.LogInformation("Step '{step}' completed successfully.", Id);
				context.Subject.Progress.ReportCompleted(Id, ParallelGroupName);

				return NodeResultStatus.Succeeded;
			}
		}

		protected override void OnBeforeExecute(IExecutionContext<SyncExecutionContext> context)
		{
			if (context.Subject.CompositeCancellationToken.StopCancellationToken.IsCancellationRequested)
			{
				context.CancelProcessing = true;
			}
			else
			{
				_logger.LogInformation("Processing step '{step}'.", Id);
				context.Subject.Progress.ReportStarted(Id, ParallelGroupName);
			}
		}
	}
}
