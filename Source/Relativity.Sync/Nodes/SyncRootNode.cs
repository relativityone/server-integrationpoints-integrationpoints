using System.Linq;
using System.Threading.Tasks;
using Banzai;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;

namespace Relativity.Sync.Nodes
{
	internal sealed class SyncRootNode : PipelineNodeBase<SyncExecutionContext>
	{
		private readonly IJobEndMetricsService _jobEndMetricsService;
		private readonly ICommand<INotificationConfiguration> _notificationCommand;
		private readonly ISyncLog _logger;

		public SyncRootNode(IJobEndMetricsService jobEndMetricsService, ICommand<INotificationConfiguration> notificationCommand, ISyncLog logger)
		{
			_jobEndMetricsService = jobEndMetricsService;
			_notificationCommand = notificationCommand;
			_logger = logger;
			Id = "SyncRoot";
		}

		protected override void OnAfterExecute(IExecutionContext<SyncExecutionContext> context)
		{
			Task metricsTask = ReportJobEndMetrics(context);
			Task notificationTask = RunNotificationCommand(context);

			Task.WaitAll(metricsTask, notificationTask);
		}

		private async Task ReportJobEndMetrics(IExecutionContext<SyncExecutionContext> context)
		{
			if (context.ParentResult.ChildResults.Any())
			{
				NodeResult validationNode = context.ParentResult.ChildResults.FirstOrDefault(x => x.Id == "Validating");
				if (validationNode != null && validationNode.Status == NodeResultStatus.Succeeded)
				{
					const string id = "Sending job end metrics";
					context.Subject.Progress.ReportStarted(id);

					ExecutionStatus status = ExecutionStatus.None;
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
					}
					if (context.CancelProcessing)
					{
						status = ExecutionStatus.Canceled;
					}

					await _jobEndMetricsService.ExecuteAsync(status).ConfigureAwait(false);
				}
			}
		}

		private async Task RunNotificationCommand(IExecutionContext<SyncExecutionContext> context)
		{
			const string id = "Sending notifications";
			context.Subject.Progress.ReportStarted(id);

			if (await _notificationCommand.CanExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false))
			{
				ExecutionResult result = await _notificationCommand.ExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false);

				// We really shouldn't be throwing if we can't return a proper result, so we'll just log the error here.
				if (result.Status == ExecutionStatus.Failed)
				{
					// result.Exception may be null, but this should not cause any issues.
					_logger.LogError(result.Exception, "Failed to send notifications: {message}", result.Message);
					context.Subject.Progress.ReportFailure(id, result.Exception);
				}
			}
		}
	}
}