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
            typeof(IPreValidationConfiguration),
            typeof(IValidationConfiguration),
            typeof(IPermissionsCheckConfiguration),
            typeof(IDestinationWorkspaceObjectTypesCreationConfiguration),
            typeof(IDataSourceSnapshotConfiguration),
            typeof(ISourceWorkspaceTagsCreationConfiguration),
            typeof(IDataDestinationInitializationConfiguration),
            typeof(IDocumentJobStartMetricsConfiguration),
            typeof(INotificationConfiguration),
            typeof(IJobStatusConsolidationConfiguration),
            typeof(IJobCleanupConfiguration),
            typeof(IAutomatedWorkflowTriggerConfiguration)
        };
    }
}
