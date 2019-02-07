using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class DestinationWorkspaceObjectTypesCreationStepTests : FailingStepsBase<IDestinationWorkspaceObjectTypesCreationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, notification
			const int expectedNumberOfExecutedSteps = 3;
			return expectedNumberOfExecutedSteps;
		}
	}
}