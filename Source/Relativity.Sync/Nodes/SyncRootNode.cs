using System;
using System.Collections.Generic;
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
		private readonly ICommand<IAutomatedWorkflowTriggerConfiguration> _automatedWfTriggerCommand;
		private readonly IJobEndMetricsServiceFactory _jobEndMetricsServiceFactory;
		private readonly ISyncLog _logger;

		public SyncRootNode(IJobEndMetricsServiceFactory jobEndMetricsServiceFactory,
			ICommand<IJobStatusConsolidationConfiguration> jobStatusConsolidationCommand,
			ICommand<INotificationConfiguration> notificationCommand,
			ICommand<IJobCleanupConfiguration> jobCleanupCommand,
			ICommand<IAutomatedWorkflowTriggerConfiguration> automatedWfTriggerCommand,
			ISyncLog logger)
		{
			_jobEndMetricsServiceFactory = jobEndMetricsServiceFactory;
			_jobStatusConsolidationCommand = jobStatusConsolidationCommand;
			_notificationCommand = notificationCommand;
			_jobCleanupCommand = jobCleanupCommand;
			_automatedWfTriggerCommand = automatedWfTriggerCommand;
			_logger = logger;
			Id = "SyncRoot";
		}

		protected override void OnAfterExecute(IExecutionContext<SyncExecutionContext> context)
		{
			ExecutionResult jobStatusConsolidationExecutionResult = RunJobStatusConsolidationAsync(context).GetAwaiter().GetResult();
			LogFailures(jobStatusConsolidationExecutionResult);

			ExecutionResult[] executionResults = ExecuteTasksInParallelWithContextSync(context,
				ReportJobEndMetricsAsync,
				RunNotificationCommandAsync,
				RunJobAutomatedWorkflowTriggerAsync);
			LogFailures(executionResults);

			ExecutionResult jobCleanupExecutionResult = RunJobCleanupAsync(context).GetAwaiter().GetResult();
			LogFailures(jobCleanupExecutionResult);

			List<SyncException> syncExceptions = executionResults
				.Concat(new[] { jobStatusConsolidationExecutionResult, jobCleanupExecutionResult })
				.Where(result => result.Status == ExecutionStatus.Failed)
				.Select(result => new SyncException(result.Message, result.Exception))
				.ToList();

			if (syncExceptions.Any())
			{
				throw new SyncException("Failures occured when finalizing the job.", new AggregateException(syncExceptions));
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

		private Task<ExecutionResult> ReportJobEndMetricsAsync(IExecutionContext<SyncExecutionContext> context)
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

					IJobEndMetricsService jobEndMetricsService = _jobEndMetricsServiceFactory.CreateJobEndMetricsService(context.Subject.CompositeCancellationToken.IsDrainStopRequested);
					return jobEndMetricsService.ExecuteAsync(status);
				}
			}
			return Task.FromResult(ExecutionResult.Skipped());
		}

		private Task<ExecutionResult> RunNotificationCommandAsync(IExecutionContext<SyncExecutionContext> context) => ExecuteCommandIfCanExecuteAsync(_notificationCommand, context);
		
		private Task<ExecutionResult> RunJobStatusConsolidationAsync(IExecutionContext<SyncExecutionContext> context) => ExecuteCommandIfCanExecuteAsync(_jobStatusConsolidationCommand, context);
		
		private Task<ExecutionResult> RunJobCleanupAsync(IExecutionContext<SyncExecutionContext> context) => ExecuteCommandIfCanExecuteAsync(_jobCleanupCommand, context);
		
		private Task<ExecutionResult> RunJobAutomatedWorkflowTriggerAsync(IExecutionContext<SyncExecutionContext> context) => ExecuteCommandIfCanExecuteAsync(_automatedWfTriggerCommand, context);

		private static async Task<ExecutionResult> ExecuteCommandIfCanExecuteAsync<T>(ICommand<T> command, IExecutionContext<SyncExecutionContext> context) where T : IConfiguration
		{
			try
			{
				if (context.Subject.CompositeCancellationToken.IsDrainStopRequested)
				{
					return ExecutionResult.Skipped();
				}

				bool canExecute = await command.CanExecuteAsync(context.Subject.CompositeCancellationToken.StopCancellationToken).ConfigureAwait(false);

				ExecutionResult executionResult = canExecute
					? await command.ExecuteAsync(context.Subject.CompositeCancellationToken).ConfigureAwait(false)
					: ExecutionResult.Skipped();

				return executionResult;
			}
			catch (Exception e)
			{
				return ExecutionResult.Failure($"Failed to execute command {command.GetType().Name}", e);
			}
		}
	}
}