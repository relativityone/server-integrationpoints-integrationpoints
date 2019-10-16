using System;
using System.Linq;
using System.Threading.Tasks;
using Banzai;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;

namespace Relativity.Sync.Nodes
{
	internal sealed class SyncRootNode : PipelineNodeBase<SyncExecutionContext>
	{
		private readonly ICommand<IJobStatusConsolidationConfiguration> _jobStatusConsolidationCommand;
		private readonly ICommand<INotificationConfiguration> _notificationCommand;
		private readonly ICommand<IJobCleanupConfiguration> _jobCleanupCommand;
		private readonly IJobEndMetricsService _jobEndMetricsService;
		private readonly ISyncLog _logger;

		public SyncRootNode(IJobEndMetricsService jobEndMetricsService,
			ICommand<IJobStatusConsolidationConfiguration> jobStatusConsolidationCommand,
			ICommand<INotificationConfiguration> notificationCommand,
			ICommand<IJobCleanupConfiguration> jobCleanupCommand,
			ISyncLog logger)
		{
			_jobEndMetricsService = jobEndMetricsService;
			_jobStatusConsolidationCommand = jobStatusConsolidationCommand;
			_notificationCommand = notificationCommand;
			_jobCleanupCommand = jobCleanupCommand;
			_logger = logger;
			Id = "SyncRoot";
		}

		protected override void OnAfterExecute(IExecutionContext<SyncExecutionContext> context)
		{
			RunJobStatusConsolidation(context);

			ExecutionResult[] executionResults = ExecuteTasksInParallelWithContextSync(context,
				ReportJobEndMetricsAsync,
				RunNotificationCommandAsync,
				RunJobCleanupAsync);

			LogFailures(executionResults);
		}

		private void RunJobStatusConsolidation(IExecutionContext<SyncExecutionContext> context)
		{
			ExecutionResult executionResult = RunJobStatusConsolidationAsync(context).GetAwaiter().GetResult();

			LogFailures(executionResult);
			if (executionResult.Status == ExecutionStatus.Failed)
			{
				throw new SyncException(executionResult.Message, executionResult.Exception);
			}
		}

		private static ExecutionResult[] ExecuteTasksInParallelWithContextSync(IExecutionContext<SyncExecutionContext> context,
			params Func<IExecutionContext<SyncExecutionContext>, Task<ExecutionResult>>[] tasks)
		{
			return Task.WhenAll(tasks.Select(task => task(context))).GetAwaiter().GetResult();
		}

		private void LogFailures(params ExecutionResult[] executionResults)
		{
			executionResults
				.Where(result => result.Status == ExecutionStatus.Failed)
				.ForEach(result => _logger.LogError(result.Exception, "After execute task failed: {message}", result.Message));

			executionResults
				.Where(result => result.Status == ExecutionStatus.CompletedWithErrors)
				.ForEach(result => _logger.LogWarning(result.Exception, "After execute task completed with errors: {message}", result.Message));
		}

		private async Task<ExecutionResult> ReportJobEndMetricsAsync(IExecutionContext<SyncExecutionContext> context)
		{
			if (context.ParentResult.ChildResults.Any())
			{
				NodeResult validationNode = context.ParentResult.ChildResults.FirstOrDefault(x => x.Id == "Validating");
				if (validationNode != null && validationNode.Status == NodeResultStatus.Succeeded)
				{
					ExecutionStatus status;
					switch (context.ParentResult.Status)
					{
						case NodeResultStatus.Succeeded:
							status = ExecutionStatus.Completed;
							break;
						case NodeResultStatus.SucceededWithErrors:
							status = ExecutionStatus.CompletedWithErrors;
							break;
						case NodeResultStatus.Failed:
							status = ExecutionStatus.Failed;
							break;
						default:
							status = ExecutionStatus.None;
							break;
					}
					if (context.CancelProcessing)
					{
						status = ExecutionStatus.Canceled;
					}

					return await _jobEndMetricsService.ExecuteAsync(status).ConfigureAwait(false);
				}
			}
			return ExecutionResult.Skipped();
		}

		private Task<ExecutionResult> RunNotificationCommandAsync(IExecutionContext<SyncExecutionContext> context) => ExecuteCommandIfCanExecuteAsync(_notificationCommand, context);

		private Task<ExecutionResult> RunJobStatusConsolidationAsync(IExecutionContext<SyncExecutionContext> context) => ExecuteCommandIfCanExecuteAsync(_jobStatusConsolidationCommand, context);

		private Task<ExecutionResult> RunJobCleanupAsync(IExecutionContext<SyncExecutionContext> context) => ExecuteCommandIfCanExecuteAsync(_jobCleanupCommand, context);

		private static async Task<ExecutionResult> ExecuteCommandIfCanExecuteAsync<T>(ICommand<T> command, IExecutionContext<SyncExecutionContext> context) where T : IConfiguration
		{
			if (await command.CanExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false))
			{
				return await command.ExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false);
			}
			return ExecutionResult.Skipped();
		}
	}
}