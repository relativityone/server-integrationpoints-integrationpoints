using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class JobCleanupStepTests : FailingStepsBase<IJobCleanupConfiguration>
	{
		protected override ICollection<Type> ExpectedExecutedSteps { get; } = new[]
		{
			typeof(IPreValidationConfiguration),
			typeof(IValidationConfiguration),
			typeof(IPermissionsCheckConfiguration),
			typeof(IDestinationWorkspaceObjectTypesCreationConfiguration),
			typeof(IDataSourceSnapshotConfiguration),
			typeof(ISourceWorkspaceTagsCreationConfiguration),
			typeof(IDestinationWorkspaceTagsCreationConfiguration),
			typeof(IDataDestinationInitializationConfiguration),
			typeof(IDocumentJobStartMetricsConfiguration),
			typeof(IDestinationWorkspaceSavedSearchCreationConfiguration),
			typeof(ISnapshotPartitionConfiguration),
			typeof(IDocumentSynchronizationConfiguration),
			typeof(IDataDestinationFinalizationConfiguration),
			typeof(IJobStatusConsolidationConfiguration),
			typeof(INotificationConfiguration),
			typeof(IAutomatedWorkflowTriggerConfiguration)
		};

		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}
	}
}