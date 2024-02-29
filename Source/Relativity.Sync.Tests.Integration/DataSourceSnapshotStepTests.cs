using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    internal sealed class DataSourceSnapshotStepTests : FailingStepsBase<IDataSourceSnapshotConfiguration>
    {
        protected override void AssertExecutedSteps(List<Type> executorTypes)
        {
            // nothing special to assert
        }

        protected override ICollection<Type> ExpectedExecutedSteps { get; } = new[]
        {
            typeof(IPreValidationConfiguration),
            typeof(IValidationConfiguration),
            typeof(IPermissionsCheckConfiguration),
            typeof(IDestinationWorkspaceObjectTypesCreationConfiguration),
            typeof(INotificationConfiguration),
            typeof(IJobStatusConsolidationConfiguration),
            typeof(IJobCleanupConfiguration),
            typeof(IAutomatedWorkflowTriggerConfiguration)
        };
    }
}
