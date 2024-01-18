using System.Linq;
using System.Threading.Tasks;
using Banzai;
using Relativity.Sync.Progress;

namespace Relativity.Sync.Nodes
{
    internal sealed class SyncMultiNode : GroupNodeBase<SyncExecutionContext>
    {
        private IExecutionContext<SyncExecutionContext> _childrenExecutionContext;
        private readonly ISyncExecutionContextFactory _contextFactory;
        private readonly string _parallelGroupName = string.Empty;

        public SyncMultiNode(ISyncExecutionContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        protected override Task<NodeResultStatus> ExecuteChildrenAsync(IExecutionContext<SyncExecutionContext> context)
        {
            _childrenExecutionContext = _contextFactory.Create(context.Subject.Progress, context.Subject.CompositeCancellationToken);
            return base.ExecuteChildrenAsync(_childrenExecutionContext);
        }

        protected override void OnAfterExecute(IExecutionContext<SyncExecutionContext> context)
        {
            context.Subject.Results.AddRange(_childrenExecutionContext.Subject.Results);
            base.OnAfterExecute(context);
        }

        protected override void OnBeforeExecute(IExecutionContext<SyncExecutionContext> context)
        {
            string mergedStep = string.Join(System.Environment.NewLine, Children.Select(x => x.Id));

            context.Subject.Progress.ReportStarted(mergedStep, _parallelGroupName);
        }
    }
}
