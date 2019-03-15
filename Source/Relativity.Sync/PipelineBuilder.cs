using System;
using Autofac;
using Banzai;
using Banzai.Autofac;
using Banzai.Factories;
using Relativity.Sync.Nodes;
using Relativity.Sync.Nodes.TagsCreation.SourceWorkspaceTagsCreation;

namespace Relativity.Sync
{
	internal sealed class PipelineBuilder : IPipelineBuilder
	{
		public void RegisterFlow(ContainerBuilder containerBuilder)
		{
			if (containerBuilder == null)
			{
				throw new ArgumentNullException(nameof(containerBuilder));
			}

			containerBuilder.RegisterBanzaiNodes(GetType().Assembly, true);

			FlowBuilder<SyncExecutionContext> flowBuilder = new FlowBuilder<SyncExecutionContext>(new AutofacFlowRegistrar(containerBuilder));

			const string pipelineName = "SYNC";

			flowBuilder.CreateFlow(pipelineName)
				.AddRoot<SyncRootNode>()
				.AddChild<PermissionsCheckNode>()
				.AddChild<ValidationNode>()
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
				.AddChild<JobCleanupNode>();

			flowBuilder.Register();

			containerBuilder.Register(c => c.Resolve<INodeFactory<SyncExecutionContext>>().BuildFlow(pipelineName)).As<INode<SyncExecutionContext>>();
		}
	}
}