using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    internal sealed class PermissionsCheckStepTests : FailingStepsBase<IPermissionsCheckConfiguration>
    {
        protected override void AssertExecutedSteps(List<Type> executorTypes)
        {
            // no need to check other steps
        }

        protected override ICollection<Type> ExpectedExecutedSteps { get; } = new[]
        {
            typeof(IPreValidationConfiguration),
            typeof(IDestinationWorkspaceObjectTypesCreationConfiguration),
            typeof(INotificationConfiguration),
            typeof(IJobCleanupConfiguration),
            typeof(IJobStatusConsolidationConfiguration),
            typeof(IAutomatedWorkflowTriggerConfiguration)
        };
    }
}
