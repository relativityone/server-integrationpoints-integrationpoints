using Autofac;
using Banzai;
using Banzai.Autofac;
using Banzai.Factories;
using Relativity.Sync.Nodes;

namespace Relativity.Sync
{
	internal sealed class PipelineBuilder : IPipelineBuilder
	{
		public void RegisterFlow(ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterBanzaiNodes(GetType().Assembly, true);

			FlowBuilder<SyncExecutionContext> flowBuilder = new FlowBuilder<SyncExecutionContext>(new AutofacFlowRegistrar(containerBuilder));

			flowBuilder.CreateFlow("SYNC")
				.AddRoot<IPipelineNode<SyncExecutionContext>>()
				.AddChild<PermissionsCheckNode>()
				.AddChild<ValidationNode>()
				.AddChild<PreviousRunCleanupNode>()
				.AddChild<TemporaryStorageInitializationNode>()
				.AddChild<DestinationWorkspaceObjectTypesCreationNode>()
				.AddChild<DataSourceSnapshotNode>()
				.AddChild<SyncMultiNode>()
				.ForLastChild()
				.AddChild<DestinationWorkspaceTagsCreationNode>()
				.AddChild<SourceWorkspaceTagsCreationNode>()
				.AddChild<DataDestinationInitializationNode>()
				.ForParent()
				.AddChild<DestinationWorkspaceSavedSearchCreationNode>()
				.AddChild<SnapshotPartitionNode>()
				.AddChild<SynchronizationNode>()
				.AddChild<DataDestinationFinalizationNode>()
				.AddChild<JobStatusConsolidationNode>()
				.AddChild<JobCleanupNode>()
				.AddChild<NotificationNode>();

			flowBuilder.Register();

			containerBuilder.Register(c => c.Resolve<INodeFactory<SyncExecutionContext>>().BuildFlow("SYNC")).As<INode<SyncExecutionContext>>();
		}
	}
}