using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class ValidationStepTests : FailingStepsBase<IValidationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// no need to check other steps
		}

		protected override ICollection<Type> ExpectedExecutedSteps { get; } = new[]
		{
			typeof(IPreValidationConfiguration),
			typeof(IDestinationWorkspaceObjectTypesCreationConfiguration),
			typeof(IPermissionsCheckConfiguration),
			typeof(INotificationConfiguration),
			typeof(IJobStatusConsolidationConfiguration),
			typeof(IJobCleanupConfiguration),
			typeof(IAutomatedWorkflowTriggerConfiguration)
		};
	}
}