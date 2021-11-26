using Banzai.Factories;
using Relativity.Sync.Nodes;
using Relativity.Sync.Nodes.SumReporting;

namespace Relativity.Sync.Pipelines
{
    internal sealed class SyncNonDocumentRunPipeline : ISyncPipeline
    {
        public void BuildFlow(IFlowBuilder<SyncExecutionContext> flowBuilder)
        {
            flowBuilder.AddRoot<SyncRootNode>()
                .AddChild<PreValidationNode>()
                .AddChild<PermissionsCheckNode>()
                .AddChild<ValidationNode>()
                .AddChild<NonDocumentObjectDataSourceSnapshotNode>()
                .AddChild<SyncMultiNode>()
                .ForLastChild()
                .AddChild<NonDocumentJobStartMetricsNode>()
                .ForParent()
                .AddChild<SnapshotPartitionNode>()
                .AddChild<ObjectLinkingSnapshotPartitionNode>();
        }
    }
}