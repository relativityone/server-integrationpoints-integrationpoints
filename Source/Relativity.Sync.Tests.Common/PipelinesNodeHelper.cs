﻿using System;
using System.Collections.Generic;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Tests.Common
{
	internal static class PipelinesNodeHelper
	{
		public static List<Type[]> GetExpectedNodesInExecutionOrder(Type pipelineType)
		{
			if (pipelineType == typeof(SyncDocumentRetryPipeline))
			{
				return GetSyncDocumentRetryPipelineExpectedNodes();
			}

			if (pipelineType == typeof(SyncDocumentRunPipeline))
			{
				return GetSyncDocumentRunPipelineExpectedNodes();
			}

			if (pipelineType == typeof(SyncImageRetryPipeline))
			{
				return GetSyncImageRetryPipelineExpectedNodes();
			}

			if (pipelineType == typeof(SyncImageRunPipeline))
			{
				return GetSyncImageRunPipelineExpectedNodes();
			}

			throw new ArgumentException($"Pipeline {pipelineType.Name} not handled in tests");
		}

		private static List<Type[]> GetSyncImageRunPipelineExpectedNodes()
		{
			return new List<Type[]>
			{
				new[] {typeof(IPreValidationConfiguration)},
				new[] {typeof(IDestinationWorkspaceObjectTypesCreationConfiguration)},
				new[] {typeof(IPermissionsCheckConfiguration)},
				new[] {typeof(IValidationConfiguration)},
				new[] {typeof(IImageDataSourceSnapshotConfiguration)},
				new[]
				{
					typeof(ISumReporterConfiguration),
					typeof(ISourceWorkspaceTagsCreationConfiguration),
					typeof(IDestinationWorkspaceTagsCreationConfiguration),
					typeof(IDataDestinationInitializationConfiguration)
				},
				new[] {typeof(IDestinationWorkspaceSavedSearchCreationConfiguration)},
				new[] {typeof(ISnapshotPartitionConfiguration)},
				new[] {typeof(IImageSynchronizationConfiguration)},
				new[] {typeof(IDataDestinationFinalizationConfiguration)},
				new[] {typeof(IJobStatusConsolidationConfiguration)},
				new[]
				{
					typeof(INotificationConfiguration),
					typeof(IAutomatedWorkflowTriggerConfiguration)
				},
				new[] {typeof(IJobCleanupConfiguration)}
			};
		}

		private static List<Type[]> GetSyncImageRetryPipelineExpectedNodes()
		{
			return new List<Type[]>
			{
				new[] {typeof(IPreValidationConfiguration)},
				new[] {typeof(IDestinationWorkspaceObjectTypesCreationConfiguration)},
				new[] {typeof(IPermissionsCheckConfiguration)},
				new[] {typeof(IValidationConfiguration)},
				new[] {typeof(IImageRetryDataSourceSnapshotConfiguration)},
				new[]
				{
					typeof(ISumReporterConfiguration),
					typeof(ISourceWorkspaceTagsCreationConfiguration),
					typeof(IDestinationWorkspaceTagsCreationConfiguration),
					typeof(IDataDestinationInitializationConfiguration)
				},
				new[] {typeof(IDestinationWorkspaceSavedSearchCreationConfiguration)},
				new[] {typeof(ISnapshotPartitionConfiguration)},
				new[] {typeof(IImageSynchronizationConfiguration)},
				new[] {typeof(IDataDestinationFinalizationConfiguration)},
				new[] {typeof(IJobStatusConsolidationConfiguration)},
				new[]
				{
					typeof(INotificationConfiguration),
					typeof(IAutomatedWorkflowTriggerConfiguration)
				},
				new[] {typeof(IJobCleanupConfiguration)}
			};
		}

		private static List<Type[]> GetSyncDocumentRunPipelineExpectedNodes()
		{
			return new List<Type[]>
			{
				new[] {typeof(IPreValidationConfiguration)},
				new[] {typeof(IDestinationWorkspaceObjectTypesCreationConfiguration)},
				new[] {typeof(IPermissionsCheckConfiguration)},
				new[] {typeof(IValidationConfiguration)},
				new[] {typeof(IDocumentDataSourceSnapshotConfiguration)},
				new[]
				{
					typeof(ISumReporterConfiguration),
					typeof(ISourceWorkspaceTagsCreationConfiguration),
					typeof(IDestinationWorkspaceTagsCreationConfiguration),
					typeof(IDataDestinationInitializationConfiguration)
				},
				new[] {typeof(IDestinationWorkspaceSavedSearchCreationConfiguration)},
				new[] {typeof(ISnapshotPartitionConfiguration)},
				new[] {typeof(IDocumentSynchronizationConfiguration)},
				new[] {typeof(IDataDestinationFinalizationConfiguration)},
				new[] {typeof(IJobStatusConsolidationConfiguration)},
				new[]
				{
					typeof(INotificationConfiguration),
					typeof(IAutomatedWorkflowTriggerConfiguration)
				},
				new[] {typeof(IJobCleanupConfiguration)}
			};
		}

		private static List<Type[]> GetSyncDocumentRetryPipelineExpectedNodes()
		{
			return new List<Type[]>
			{
				new[] {typeof(IPreValidationConfiguration)},
				new[] {typeof(IDestinationWorkspaceObjectTypesCreationConfiguration)},
				new[] {typeof(IPermissionsCheckConfiguration)},
				new[] {typeof(IValidationConfiguration)},
				new[] {typeof(IDocumentRetryDataSourceSnapshotConfiguration)},
				new[]
				{
					typeof(ISumReporterConfiguration),
					typeof(ISourceWorkspaceTagsCreationConfiguration),
					typeof(IDestinationWorkspaceTagsCreationConfiguration),
					typeof(IDataDestinationInitializationConfiguration)
				},
				new[] {typeof(IDestinationWorkspaceSavedSearchCreationConfiguration)},
				new[] {typeof(ISnapshotPartitionConfiguration)},
				new[] {typeof(IDocumentSynchronizationConfiguration)},
				new[] {typeof(IDataDestinationFinalizationConfiguration)},
				new[] {typeof(IJobStatusConsolidationConfiguration)},
				new[]
				{
					typeof(INotificationConfiguration),
					typeof(IAutomatedWorkflowTriggerConfiguration)
				},
				new[] {typeof(IJobCleanupConfiguration)}
			};
		}
	}
}
