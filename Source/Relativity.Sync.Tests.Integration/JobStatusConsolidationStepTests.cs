using System;
using System.Collections.Generic;
using NUnit.Framework;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class JobStatusConsolidationStepTests : FailingStepsBase<IJobStatusConsolidationConfiguration>
	{
		protected override void AssertExecutedSteps(List<Type> executorTypes)
		{
			// nothing special to assert
		}

		protected override int ExpectedNumberOfExecutedSteps()
		{
			// validation, permissions, object types, snapshot,
			// source tags, dest tags, data destination init, sum reporting
			// saved search, snapshot partition, sync, data destination finalization,
			const int expectedNumberOfExecutedSteps = 12;
			return expectedNumberOfExecutedSteps;
		}
	}
}