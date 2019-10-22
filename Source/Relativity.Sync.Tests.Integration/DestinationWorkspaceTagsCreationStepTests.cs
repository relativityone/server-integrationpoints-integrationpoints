using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class DestinationWorkspaceTagsCreationStepTests : FailingStepsBase<IDestinationWorkspaceTagsCreationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			executorTypes.Should().Contain(x => x == typeof(ISourceWorkspaceTagsCreationConfiguration));
			executorTypes.Should().Contain(x => x == typeof(IDataDestinationInitializationConfiguration));
		}

		protected override ICollection<Type> ExpectedExecutedSteps { get; } = new[]
		{
			typeof(IValidationConfiguration),
			typeof(IPermissionsCheckConfiguration),
			typeof(IDestinationWorkspaceObjectTypesCreationConfiguration),
			typeof(IDataSourceSnapshotConfiguration),
			typeof(ISourceWorkspaceTagsCreationConfiguration),
			typeof(IDataDestinationInitializationConfiguration),
			typeof(ISumReporterConfiguration),
			typeof(INotificationConfiguration),
			typeof(IJobStatusConsolidationConfiguration),
			typeof(IJobCleanupConfiguration)
		};

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot,
			// source workspace tags, data destination init, sum reporting,
			// notification, job status consolidation, cleanup
			const int expectedNumberOfExecutedSteps = 10;
			return expectedNumberOfExecutedSteps;
		}
	}
}