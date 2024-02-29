using Banzai.Factories;

namespace Relativity.Sync.Pipelines
{
    internal interface ISyncPipeline
    {
        void BuildFlow(IFlowBuilder<SyncExecutionContext> flowBuilder);
    }
}
