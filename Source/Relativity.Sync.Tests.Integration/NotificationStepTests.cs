﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class NotificationStepTests : FailingStepsBase<INotificationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override ICollection<Type> ExpectedExecutedSteps { get; } = new[]
		{
			typeof(IValidationConfiguration),
			typeof(IPermissionsCheckConfiguration),
			typeof(IDestinationWorkspaceObjectTypesCreationConfiguration),
			typeof(IDocumentDataSourceSnapshotConfiguration),
			typeof(ISourceWorkspaceTagsCreationConfiguration),
			typeof(IDestinationWorkspaceTagsCreationConfiguration),
			typeof(IDataDestinationInitializationConfiguration),
			typeof(ISumReporterConfiguration),
			typeof(IDestinationWorkspaceSavedSearchCreationConfiguration),
			typeof(ISnapshotPartitionConfiguration),
			typeof(IDocumentSynchronizationConfiguration),
			typeof(IDataDestinationFinalizationConfiguration),
			typeof(IJobStatusConsolidationConfiguration),
			typeof(IJobCleanupConfiguration),
			typeof(IAutomatedWorkflowTriggerConfiguration)
		};
	}
}