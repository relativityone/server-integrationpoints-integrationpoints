using System.Threading.Tasks;
using Banzai;

namespace Relativity.Sync.Tests.Unit.Stubs
{
	internal sealed class NodeWithProgressStub : Node<SyncExecutionContext>
	{
		protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<SyncExecutionContext> context)
		{
			context.Subject.Progress.Report(SyncJobState.Start(Id));
			return Task.FromResult(NodeResultStatus.Succeeded);
		}
	}
}