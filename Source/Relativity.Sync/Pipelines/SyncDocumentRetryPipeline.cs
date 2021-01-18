﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Banzai.Factories;
using Relativity.Sync.Nodes;
using Relativity.Sync.Nodes.SumReporting;

namespace Relativity.Sync.Pipelines
{
	internal sealed class SyncDocumentRetryPipeline : ISyncPipeline
	{
		public void BuildFlow(IFlowBuilder<SyncExecutionContext> flowBuilder)
		{
			flowBuilder.AddRoot<SyncRootNode>()
				.AddChild<PreValidationNode>()
				.AddChild<DestinationWorkspaceObjectTypesCreationNode>()
				.AddChild<PermissionsCheckNode>()
				.AddChild<ValidationNode>()
				.AddChild<DocumentRetryDataSourceSnapshotNode>()
				.AddChild<SyncMultiNode>()
				.ForLastChild()
				.AddChild<JobStartMetricsNode>()
				.AddChild<DestinationWorkspaceTagsCreationNode>()
				.AddChild<SourceWorkspaceTagsCreationNode>()
				.AddChild<DataDestinationInitializationNode>()
				.ForParent()
				.AddChild<DestinationWorkspaceSavedSearchCreationNode>()
				.AddChild<SnapshotPartitionNode>()
				.AddChild<DocumentSynchronizationNode>()
				.AddChild<DataDestinationFinalizationNode>();
		}
	}
}