using System;
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
			typeof(IDataSourceSnapshotConfiguration),
			typeof(ISourceWorkspaceTagsCreationConfiguration),
			typeof(IDestinationWorkspaceTagsCreationConfiguration),
			typeof(IDataDestinationInitializationConfiguration),
			typeof(ISumReporterConfiguration),
			typeof(IDestinationWorkspaceSavedSearchCreationConfiguration),
			typeof(ISnapshotPartitionConfiguration),
			typeof(ISynchronizationConfiguration),
			typeof(IDataDestinationFinalizationConfiguration),
			typeof(IJobStatusConsolidationConfiguration),
			typeof(IJobCleanupConfiguration)
		};

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot,
			// source tags, dest tags, data destination init, sum reporting,
			// saved search, snapshot partition, sync, data destination finalization,
			// job status consolidation, job cleanup
			const int expectedNumberOfExecutedSteps = 14;
			return expectedNumberOfExecutedSteps;
		}
	}
}