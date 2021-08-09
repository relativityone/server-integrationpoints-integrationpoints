using Banzai.Factories;
using Relativity.Sync.Nodes;
using Relativity.Sync.Nodes.SumReporting;

namespace Relativity.Sync.Pipelines
{
	internal sealed class SyncImageRunPipeline : ISyncPipeline
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
				.AddChild<ImageJobStartMetricsNode>()
				.AddChild<DestinationWorkspaceTagsCreationNode>()
				.AddChild<SourceWorkspaceTagsCreationNode>()
				.AddChild<DataDestinationInitializationNode>()
				.ForParent()
				.AddChild<DestinationWorkspaceSavedSearchCreationNode>()
				.AddChild<SnapshotPartitionNode>()
				.AddChild<ImageSynchronizationNode>()
				.AddChild<DataDestinationFinalizationNode>();
		}
	}
}
