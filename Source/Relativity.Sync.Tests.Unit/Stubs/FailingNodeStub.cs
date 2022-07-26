using System;
using System.Threading.Tasks;
using Banzai;

namespace Relativity.Sync.Tests.Unit.Stubs
{
    internal sealed class FailingNodeStub<T> : Node<SyncExecutionContext> where T : Exception, new()
    {
        public FailingNodeStub(ExecutionOptions localOptions) : base(localOptions)
        {
        }

        protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<SyncExecutionContext> context)
        {
            throw new T();
        }
    }
}