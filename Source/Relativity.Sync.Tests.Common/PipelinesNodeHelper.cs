using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
				return new List<Type[]>
				{
					new[] {typeof(IDestinationWorkspaceObjectTypesCreationConfiguration)},
					new[] {typeof(IPermissionsCheckConfiguration)},
					new[] {typeof(IValidationConfiguration)},
					new[] {typeof(IRetryDataSourceSnapshotConfiguration)},
					new[]
					{
						typeof(ISumReporterConfiguration),
						typeof(ISourceWorkspaceTagsCreationConfiguration),
						typeof(IDestinationWorkspaceTagsCreationConfiguration),
						typeof(IDataDestinationInitializationConfiguration)
					},
					new[] {typeof(IDestinationWorkspaceSavedSearchCreationConfiguration)},
					new[] {typeof(ISnapshotPartitionConfiguration)},
					new[] {typeof(ISynchronizationConfiguration)},
					new[] {typeof(IDataDestinationFinalizationConfiguration)},
					new[] {typeof(IJobStatusConsolidationConfiguration)},
					new[]
					{
						typeof(INotificationConfiguration),
						typeof(IAutomatedWorkflowTriggerConfiguration)
					},
					new[] {typeof(IJobCleanupConfiguration) }
				};
			}

			if (pipelineType == typeof(SyncDocumentRunPipeline))
			{
				return new List<Type[]>
				{
					new[] {typeof(IDestinationWorkspaceObjectTypesCreationConfiguration)},
					new[] {typeof(IPermissionsCheckConfiguration)},
					new[] {typeof(IValidationConfiguration)},
					new[] {typeof(IDataSourceSnapshotConfiguration)},
					new[]
					{
						typeof(ISumReporterConfiguration),
						typeof(ISourceWorkspaceTagsCreationConfiguration),
						typeof(IDestinationWorkspaceTagsCreationConfiguration),
						typeof(IDataDestinationInitializationConfiguration)
					},
					new[] {typeof(IDestinationWorkspaceSavedSearchCreationConfiguration)},
					new[] {typeof(ISnapshotPartitionConfiguration)},
					new[] {typeof(ISynchronizationConfiguration)},
					new[] {typeof(IDataDestinationFinalizationConfiguration)},
					new[] {typeof(IJobStatusConsolidationConfiguration)},
					new[]
					{
					typeof(INotificationConfiguration),
					typeof(IAutomatedWorkflowTriggerConfiguration)
				},
				new[] {typeof(IJobCleanupConfiguration) }
				};
			}

			throw new ArgumentException($"Pipeline {pipelineType.Name} not handled in tests");
		}
	}
}
