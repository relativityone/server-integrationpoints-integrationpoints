using Banzai.Factories;
using Relativity.Sync.Nodes;
using Relativity.Sync.Nodes.SumReporting;

namespace Relativity.Sync.Pipelines
{
    internal sealed class IAPI2_SyncDocumentRunPipeline : ISyncPipeline
    {
        public void BuildFlow(IFlowBuilder<SyncExecutionContext> flowBuilder)
        {
            flowBuilder.AddRoot<SyncRootNode>()
                .AddChild<PreValidationNode>()
                .AddChild<DestinationWorkspaceObjectTypesCreationNode>()
                .AddChild<PermissionsCheckNode>()
                .AddChild<ValidationNode>()
                .AddChild<DataSourceSnapshotNode>()
                .AddChild<SyncMultiNode>()
                .ForLastChild()
                .AddChild<DocumentJobStartMetricsNode>()
                .AddChild<DestinationWorkspaceTagsCreationNode>()
                .AddChild<SourceWorkspaceTagsCreationNode>()
                .AddChild<DataDestinationInitializationNode>()
                .ForParent()
                .AddChild<DestinationWorkspaceSavedSearchCreationNode>()
                .AddChild<ConfigureDocumentSynchronizationNode>()
                .AddChild<SnapshotPartitionNode>()
                .AddChild<BatchDataSourcePreparationNode>()
                .AddChild<DocumentSynchronizationMonitorNode>()
                .AddChild<DataDestinationFinalizationNode>();
        }
    }
}
