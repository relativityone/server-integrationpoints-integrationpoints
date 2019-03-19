using System;
using System.Linq;
using System.Threading.Tasks;
using Banzai;

namespace Relativity.Sync.Nodes
{
	internal sealed class SyncMultiNode : GroupNodeBase<SyncExecutionContext>
	{
		private readonly ISyncExecutionContextFactory _contextFactory;

		public SyncMultiNode(ISyncExecutionContextFactory contextFactory)
		{
			_contextFactory = contextFactory;
		}

		protected override Task<NodeResultStatus> ExecuteChildrenAsync(IExecutionContext<SyncExecutionContext> context)
		{
			IExecutionContext<SyncExecutionContext> childrenExecutionContext = _contextFactory.Create(new EmptyProgress<SyncProgress>(), context.Subject.CancellationToken);
			return base.ExecuteChildrenAsync(childrenExecutionContext);
		}

		protected override void OnBeforeExecute(IExecutionContext<SyncExecutionContext> context)
		{
			string mergedStep = string.Join(Environment.NewLine, Children.Select(x => x.Id));

			context.Subject.Progress.Report(new SyncProgress(mergedStep));
		}
	}
}