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
			// validation, permissions, cleanup, storage init, notification
			const int expectedNumberOfExecutedSteps = 5;
			return expectedNumberOfExecutedSteps;
		}
	}
}