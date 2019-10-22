using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class DestinationWorkspaceSavedSearchCreationStepTests : FailingStepsBase<IDestinationWorkspaceSavedSearchCreationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override ICollection<Type> ExpectedExecutedSteps { get; } = new[]
		{
			typeof(IPermissionsCheckConfiguration),
			typeof(IValidationConfiguration),
			typeof(IDestinationWorkspaceObjectTypesCreationConfiguration),
			typeof(IDataSourceSnapshotConfiguration),
			typeof(ISumReporterConfiguration),
			typeof(ISourceWorkspaceTagsCreationConfiguration),
			typeof(IDestinationWorkspaceTagsCreationConfiguration),
			typeof(IDataDestinationInitializationConfiguration),
			typeof(IJobStatusConsolidationConfiguration),
			typeof(INotificationConfiguration),
			typeof(IJobCleanupConfiguration)
		};

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// permissions, validation, object types, snapshot
			// sum reporting, source workspace tags, destination workspace tags, data destination initialization,
			// job status consolidation, notification, job cleanup
			const int expectedNumberOfExecutedSteps = 11;
			return expectedNumberOfExecutedSteps;
		}
	}
}