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

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// Notification, cleanup, job status consolidation
			const int expectedNumberOfExecutedSteps = 3;
			return expectedNumberOfExecutedSteps;
		}
	}
}
