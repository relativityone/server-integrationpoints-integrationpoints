﻿using System;
using Autofac;
using Banzai;
using Banzai.Autofac;
using Banzai.Factories;
using Relativity.Sync.Nodes;
using Relativity.Sync.Nodes.SumReporting;

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
				.AddChild<DestinationWorkspaceObjectTypesCreationNode>()
				.AddChild<PermissionsCheckNode>()
				.AddChild<ValidationNode>()
				.AddChild<DataSourceSnapshotNode>()
				.AddChild<SyncMultiNode>()
				.ForLastChild()
				.AddChild<JobStartMetricsNode>()
				.AddChild<DestinationWorkspaceTagsCreationNode>()
				.AddChild<SourceWorkspaceTagsCreationNode>()
				.AddChild<DataDestinationInitializationNode>()
				.ForParent()
				.AddChild<DestinationWorkspaceSavedSearchCreationNode>()
				.AddChild<SnapshotPartitionNode>()
				.AddChild<SynchronizationNode>()
				.AddChild<DataDestinationFinalizationNode>();

			flowBuilder.Register();

			containerBuilder.Register(c => c.Resolve<INodeFactory<SyncExecutionContext>>().BuildFlow(pipelineName)).As<INode<SyncExecutionContext>>();
		}
	}
}