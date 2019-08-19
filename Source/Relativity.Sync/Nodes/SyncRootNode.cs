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
		private readonly string _parallelGroupName = string.Empty;

		public SyncRootNode(IJobEndMetricsService jobEndMetricsService, ICommand<INotificationConfiguration> notificationCommand)
		{
			_jobEndMetricsService = jobEndMetricsService;
			_notificationCommand = notificationCommand;
			Id = "SyncRoot";
		}

		protected override void OnAfterExecute(IExecutionContext<SyncExecutionContext> context)
		{
			Task metricsTask = ReportJobEndMetrics(context);
			Task notificationTask = RunNotificationCommand(context);

			Task.WhenAll(metricsTask, notificationTask).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private async Task ReportJobEndMetrics(IExecutionContext<SyncExecutionContext> context)
		{
			if (context.ParentResult.ChildResults.Any())
			{
				NodeResult validationNode = context.ParentResult.ChildResults.FirstOrDefault(x => x.Id == "Validating");
				if (validationNode != null && validationNode.Status == NodeResultStatus.Succeeded)
				{
					const string id = "Sending job end metrics";
					context.Subject.Progress.ReportStarted(id, _parallelGroupName);

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

					await _jobEndMetricsService.ExecuteAsync(status).ConfigureAwait(false);
				}
			}
		}

		private async Task RunNotificationCommand(IExecutionContext<SyncExecutionContext> context)
		{
			if (await _notificationCommand.CanExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false))
			{
				await _notificationCommand.ExecuteAsync(context.Subject.CancellationToken).ConfigureAwait(false);
			}
		}
	}
}