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

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// permissions, notification, job status consolidation, cleanup
			const int expectedNumberOfExecutedSteps = 4;
			return expectedNumberOfExecutedSteps;
		}
	}
}