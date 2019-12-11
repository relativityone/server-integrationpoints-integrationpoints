using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class AutomatedWorkflowExecutorConstrains : IExecutionConstrains<IAutomatedWorkflowTriggerConfiguration>
	{
		public Task<bool> CanExecuteAsync(IAutomatedWorkflowTriggerConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(configuration.SynchronizationExecutionResult != null &&
									(configuration.SynchronizationExecutionResult.Status == ExecutionStatus.Completed ||
									configuration.SynchronizationExecutionResult.Status == ExecutionStatus.CompletedWithErrors ||
									configuration.SynchronizationExecutionResult.Status == ExecutionStatus.Failed));
		}
	}
}