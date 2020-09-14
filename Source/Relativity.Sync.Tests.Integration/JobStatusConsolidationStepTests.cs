﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Services.Security;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class JobStatusConsolidationStepTests : FailingStepsBase<IJobStatusConsolidationConfiguration>
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
			typeof(ISynchronizationConfiguration),
			typeof(IDataDestinationFinalizationConfiguration),
			typeof(INotificationConfiguration),
			typeof(IJobCleanupConfiguration),
			typeof(IAutomatedWorkflowTriggerConfiguration)
		};
	}
}