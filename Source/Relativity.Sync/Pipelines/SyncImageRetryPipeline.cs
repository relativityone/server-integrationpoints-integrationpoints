using Banzai.Factories;
using Relativity.Sync.Nodes;
using Relativity.Sync.Nodes.SumReporting;

namespace Relativity.Sync.Pipelines
{
	internal sealed class SyncImageRetryPipeline : ISyncPipeline
	{
		public void BuildFlow(IFlowBuilder<SyncExecutionContext> flowBuilder)
		{
			flowBuilder.AddRoot<SyncRootNode>()
				.AddChild<PreValidationNode>()
				.AddChild<DestinationWorkspaceObjectTypesCreationNode>()
				.AddChild<PermissionsCheckNode>()
				.AddChild<ValidationNode>()
				.AddChild<SourceWorkspaceObjectTypesCreationNode>()
				.AddChild<ImageRetryDataSourceSnapshotNode>()
				.AddChild<SyncMultiNode>()
				.ForLastChild()
				.AddChild<JobStartMetricsNode>()
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
