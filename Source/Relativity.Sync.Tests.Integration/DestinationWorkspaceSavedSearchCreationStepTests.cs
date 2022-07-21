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
            typeof(IPreValidationConfiguration),
            typeof(IPermissionsCheckConfiguration),
            typeof(IValidationConfiguration),
            typeof(IDestinationWorkspaceObjectTypesCreationConfiguration),
            typeof(IDataSourceSnapshotConfiguration),
            typeof(IDocumentJobStartMetricsConfiguration),
            typeof(ISourceWorkspaceTagsCreationConfiguration),
            typeof(IDestinationWorkspaceTagsCreationConfiguration),
            typeof(IDataDestinationInitializationConfiguration),
            typeof(IJobStatusConsolidationConfiguration),
            typeof(INotificationConfiguration),
            typeof(IJobCleanupConfiguration),
            typeof(IAutomatedWorkflowTriggerConfiguration)
        };
    }
}