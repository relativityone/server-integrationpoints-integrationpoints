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
			// validation, permissions, object types, snapshot, source tags, dest tags,
			// data destination init, snapshot partition, sync, saved search,
			// data destination finalization, notification
			const int expectedNumberOfExecutedSteps = 12;
			return expectedNumberOfExecutedSteps;
		}
	}
}